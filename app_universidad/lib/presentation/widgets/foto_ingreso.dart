import 'package:flutter/material.dart';

import '../../core/constants/sharepoint_constants.dart';

/// Miniatura de la foto de un ingreso, resuelta contra el servicio documental.
///
/// La API guarda solo `sharepoint:{uid}`; la URL se compone aquí y la descarga
/// necesita la cabecera `ProviderKey`. Sin ella el servicio responde 401 y la
/// imagen quedaría rota sin explicación.
///
/// Si no hay foto, cae al icono que se venía mostrando: la mayoría de los
/// registros históricos no la tienen y no deben verse como errores.
class FotoIngresoAvatar extends StatelessWidget {
  final String? referencia;
  final double radio;

  /// Icono y color del marcador cuando no hay foto.
  final IconData iconoSinFoto;
  final Color colorSinFoto;

  /// Texto de la vista ampliada, para saber de quién es la foto.
  final String? titulo;

  const FotoIngresoAvatar({
    super.key,
    required this.referencia,
    this.radio = 22,
    this.iconoSinFoto = Icons.person,
    this.colorSinFoto = Colors.grey,
    this.titulo,
  });

  bool get _tieneFoto => SharepointConstants.isRef(referencia);

  @override
  Widget build(BuildContext context) {
    if (!_tieneFoto) {
      return CircleAvatar(
        radius: radio,
        backgroundColor: colorSinFoto,
        child: Icon(iconoSinFoto, color: Colors.white, size: radio),
      );
    }

    final url = SharepointConstants.resolveUrl(referencia)!;

    return GestureDetector(
      onTap: () => _abrirVistaAmpliada(context, url),
      child: CircleAvatar(
        radius: radio,
        backgroundColor: Colors.black12,
        child: ClipOval(
          child: Image.network(
            url,
            headers: SharepointConstants.downloadHeaders,
            width: radio * 2,
            height: radio * 2,
            fit: BoxFit.cover,
            loadingBuilder: (context, child, progreso) {
              if (progreso == null) return child;
              return SizedBox(
                width: radio,
                height: radio,
                child: const CircularProgressIndicator(strokeWidth: 2),
              );
            },
            errorBuilder: (context, error, stack) => Icon(
              Icons.broken_image_outlined,
              size: radio,
              color: Colors.grey[600],
            ),
          ),
        ),
      ),
    );
  }

  void _abrirVistaAmpliada(BuildContext context, String url) {
    showDialog<void>(
      context: context,
      barrierColor: Colors.black87,
      builder: (ctx) => Dialog(
        backgroundColor: Colors.transparent,
        insetPadding: const EdgeInsets.all(12),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (titulo != null && titulo!.isNotEmpty)
              Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: Text(
                  titulo!,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            Flexible(
              child: ClipRRect(
                borderRadius: BorderRadius.circular(12),
                // Permite ampliar con los dedos: en una foto de portería a
                // veces hay que acercarse a la cara o al documento.
                child: InteractiveViewer(
                  minScale: 1,
                  maxScale: 4,
                  child: Image.network(
                    url,
                    headers: SharepointConstants.downloadHeaders,
                    fit: BoxFit.contain,
                    loadingBuilder: (context, child, progreso) {
                      if (progreso == null) return child;
                      final total = progreso.expectedTotalBytes;
                      return SizedBox(
                        height: 220,
                        child: Center(
                          child: CircularProgressIndicator(
                            color: Colors.white,
                            value: total != null
                                ? progreso.cumulativeBytesLoaded / total
                                : null,
                          ),
                        ),
                      );
                    },
                    errorBuilder: (context, error, stack) => const SizedBox(
                      height: 220,
                      child: Center(
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(Icons.broken_image_outlined,
                                size: 48, color: Colors.white70),
                            SizedBox(height: 10),
                            Text(
                              'No se pudo cargar la foto.\n'
                              'Revisa la conexión con el servicio de archivos.',
                              textAlign: TextAlign.center,
                              style: TextStyle(color: Colors.white70),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ),
            const SizedBox(height: 10),
            TextButton.icon(
              onPressed: () => Navigator.pop(ctx),
              icon: const Icon(Icons.close, color: Colors.white),
              label: const Text('Cerrar', style: TextStyle(color: Colors.white)),
            ),
          ],
        ),
      ),
    );
  }
}
