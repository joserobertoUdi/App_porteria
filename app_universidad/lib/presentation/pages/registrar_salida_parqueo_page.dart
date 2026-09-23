import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme/app_theme.dart';
import '../widgets/udi_appbar_logo.dart';
import '../../core/utils/uppercase_text_formatter.dart';
import '../../data/datasources/parqueo_remote_data_source.dart';
import '../../data/models/parqueo_models.dart';

class RegistrarSalidaParqueoPage extends StatefulWidget {
  const RegistrarSalidaParqueoPage({super.key});

  @override
  State<RegistrarSalidaParqueoPage> createState() => _RegistrarSalidaParqueoPageState();
}

class _RegistrarSalidaParqueoPageState extends State<RegistrarSalidaParqueoPage> {
  final _placaController = TextEditingController();

  bool _buscando = false;
  bool _registrando = false;
  ParqueoActivoModel? _activo;
  int? _registroId;

  @override
  void dispose() {
    _placaController.dispose();
    super.dispose();
  }

  Future<void> _buscarActivo() async {
    final placa = _placaController.text.trim().toUpperCase();
    if (placa.isEmpty) return;

    setState(() {
      _buscando = true;
      _activo = null;
      _registroId = null;
    });

    try {
      final api = context.read<ParqueoRemoteDataSource>();
      final activos = await api.obtenerActivos();
      final encontrado = activos.where((a) => a.matricula == placa).toList();

      setState(() {
        _buscando = false;
        if (encontrado.isNotEmpty) {
          _activo = encontrado.first;
          _registroId = _activo!.idRegistro;
        }
      });

      if (encontrado.isEmpty && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('No hay entrada activa para esa placa')),
        );
      }
    } catch (e) {
      if (mounted) {
        setState(() => _buscando = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error al buscar: $e')),
        );
      }
    }
  }

  Future<void> _registrarSalida() async {
    if (_registroId == null) return;

    setState(() => _registrando = true);

    final api = context.read<ParqueoRemoteDataSource>();
    try {
      await api.registrarSalida(_registroId!);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Salida registrada'), backgroundColor: Colors.green),
        );
        Navigator.pop(context);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error: ${e.toString().replaceFirst('Exception: ', '')}')),
        );
      }
    } finally {
      if (mounted) setState(() => _registrando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const UdiAppBarLogo(width: 40),
            const SizedBox(width: 8),
            const Text('Salida Parqueo', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          ],
        ),
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: const BoxDecoration(gradient: AppTheme.udiGradient),
        child: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text('Buscar por placa',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white)),
                const SizedBox(height: 10),
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _placaController,
                        textCapitalization: TextCapitalization.characters,
                        inputFormatters: [UppercaseTextFormatter()],
                        style: const TextStyle(color: Colors.black87),
                        textInputAction: TextInputAction.search,
                        onSubmitted: (_) => _buscarActivo(),
                        decoration: InputDecoration(
                          hintText: 'Placa',
                          hintStyle: TextStyle(color: Colors.grey[600]),
                          filled: true,
                          fillColor: Colors.white,
                          prefixIcon: Icon(Icons.search, color: Colors.grey[600]),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                            borderSide: const BorderSide(color: Colors.white70),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                            borderSide: const BorderSide(color: Colors.white, width: 2),
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(width: 10),
                    ElevatedButton(
                      onPressed: _buscando ? null : _buscarActivo,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.white,
                        foregroundColor: AppTheme.udiRed,
                        padding: const EdgeInsets.symmetric(vertical: 15, horizontal: 15),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      child: _buscando
                          ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2))
                          : const Icon(Icons.person_search),
                    ),
                  ],
                ),
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 20),
                  child: Divider(color: Colors.white54),
                ),
                if (_activo != null) ...[
                  Container(
                    padding: const EdgeInsets.all(15),
                    decoration: BoxDecoration(
                      color: Colors.white.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        _infoRow('Propietario', _activo!.nombreCompleto),
                        _infoRow('Carnet', _activo!.documentoIdentidad),
                        _infoRow('Placa', _activo!.matricula),
                        _infoRow('Marca', _activo!.marca ?? '-'),
                        _infoRow('Modelo', _activo!.modelo ?? '-'),
                        _infoRow('Color', _activo!.color ?? '-'),
                        _infoRow('Puerta', _activo!.puertaAcceso),
                        _infoRow('Ingreso', _activo!.fechaIngreso.toString().substring(0, 19)),
                      ],
                    ),
                  ),
                  const SizedBox(height: 30),
                  ElevatedButton(
                    onPressed: _registrando ? null : _registrarSalida,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.white,
                      foregroundColor: AppTheme.udiRed,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30)),
                    ),
                    child: _registrando
                        ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2))
                        : const Text('CONFIRMAR SALIDA', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _infoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          SizedBox(
            width: 100,
            child: Text(label, style: const TextStyle(color: Colors.white70, fontWeight: FontWeight.bold)),
          ),
          Expanded(
            child: Text(value, style: const TextStyle(color: Colors.white, fontSize: 16)),
          ),
        ],
      ),
    );
  }
}
