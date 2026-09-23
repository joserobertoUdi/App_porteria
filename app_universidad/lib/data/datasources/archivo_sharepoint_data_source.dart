import 'dart:convert';
import 'dart:io';

import 'package:http/http.dart' as http;

import '../../core/constants/sharepoint_constants.dart';

/// Resultado de subir un archivo a SharepointApi.
class ArchivoSubido {
  /// Identificador que devuelve el servicio.
  final String uid;

  /// Referencia que se guarda en la base de portería (`sharepoint:{uid}`).
  final String referencia;

  final String nombreArchivo;

  const ArchivoSubido({
    required this.uid,
    required this.referencia,
    required this.nombreArchivo,
  });
}

/// Excepción con un mensaje ya redactado para mostrar al usuario.
class ArchivoException implements Exception {
  final String message;
  const ArchivoException(this.message);

  @override
  String toString() => message;
}

/// Cliente del servicio central de gestión documental.
///
/// No reutiliza [ApiClient] porque apunta a otro host y se autentica con una
/// cabecera propia (`ProviderKey`) en vez del JWT de ServiciosGenerales.
class ArchivoSharepointDataSource {
  final http.Client _client;

  ArchivoSharepointDataSource({http.Client? client})
      : _client = client ?? http.Client();

  /// Sube una foto mediante `POST /api/Archivos` (multipart/form-data).
  ///
  /// [referenciaOrigen] identifica al registro dueño del archivo; aquí se usa
  /// el documento de identidad, para poder rastrear después de quién es la foto.
  Future<ArchivoSubido> subirFoto({
    required File archivo,
    required String referenciaOrigen,
    required String usuarioRegistro,
  }) async {
    if (!await archivo.exists()) {
      throw const ArchivoException('La foto seleccionada ya no existe.');
    }

    final tamano = await archivo.length();
    if (tamano == 0) {
      throw const ArchivoException('La foto está vacía.');
    }
    if (tamano > SharepointConstants.maxFileSizeBytes) {
      final mb = (tamano / (1024 * 1024)).toStringAsFixed(1);
      throw ArchivoException('La foto pesa $mb MB y supera el límite de '
          '${SharepointConstants.maxFileSizeBytes ~/ (1024 * 1024)} MB.');
    }

    final nombreArchivo = archivo.uri.pathSegments.last;
    final uri = Uri.parse(
        '${SharepointConstants.baseUrl}${SharepointConstants.uploadEndpoint}');

    final peticion = http.MultipartRequest('POST', uri)
      ..headers[SharepointConstants.providerKeyHeader] =
          SharepointConstants.providerKey
      ..fields['Contenedor'] = SharepointConstants.contenedor
      ..fields['EntidadOrigen'] = SharepointConstants.entidadOrigen
      ..fields['ReferenciaOrigen'] = referenciaOrigen
      ..fields['UsuarioRegistro'] = usuarioRegistro
      ..files.add(await http.MultipartFile.fromPath('Archivo', archivo.path,
          filename: nombreArchivo));

    http.Response respuesta;
    try {
      final streamed = await _client
          .send(peticion)
          .timeout(const Duration(minutes: 3));
      respuesta = await http.Response.fromStream(streamed);
    } on SocketException {
      throw ArchivoException('No se pudo conectar con el servicio de archivos '
          '(${SharepointConstants.baseUrl}). Revisa la red del equipo.');
    } catch (e) {
      throw ArchivoException('Error de red al subir la foto: $e');
    }

    final cuerpo = _decodificar(respuesta.body);

    if (respuesta.statusCode < 200 || respuesta.statusCode >= 300) {
      throw ArchivoException(_mensajeError(respuesta.statusCode, cuerpo));
    }

    // Contrato de SharepointApi (ApiSuccessResponse<ArchivoDto>):
    // { exito, mensaje, detalles, datos: { uid, nombreArchivo, ... } }
    final datos = cuerpo?['datos'];
    final uid = datos is Map ? datos['uid'] as String? : null;

    if (uid == null || uid.isEmpty) {
      throw ArchivoException(
          'El servicio no devolvió el uid del archivo. Respuesta: ${respuesta.body}');
    }

    return ArchivoSubido(
      uid: uid,
      referencia: SharepointConstants.buildRef(uid),
      nombreArchivo: (datos is Map ? datos['nombreArchivo'] as String? : null) ??
          nombreArchivo,
    );
  }

  Map<String, dynamic>? _decodificar(String body) {
    if (body.isEmpty) return null;
    try {
      final decodificado = jsonDecode(body);
      return decodificado is Map<String, dynamic> ? decodificado : null;
    } catch (_) {
      return null;
    }
  }

  /// SharepointApi responde siempre con `ApiErrorResponse`, que trae el motivo
  /// real en `mensaje`: contenedor inexistente, extensión no permitida, archivo
  /// por encima del tope. Sin esto solo se vería el código HTTP.
  String _mensajeError(int status, Map<String, dynamic>? cuerpo) {
    final mensaje = cuerpo?['mensaje'] as String?;

    return switch (status) {
      401 => 'El servicio de archivos rechazó la clave de acceso.',
      400 => mensaje ?? 'La foto no fue aceptada por el servicio.',
      413 => 'La foto es demasiado grande para el servidor.',
      429 => 'Demasiadas subidas seguidas; espera un minuto.',
      502 => 'El servicio no pudo guardar en SharePoint: ${mensaje ?? ''}',
      _ => mensaje ?? 'El servicio de archivos respondió $status.',
    };
  }
}
