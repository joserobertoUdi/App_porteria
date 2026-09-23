import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants/error_messages.dart';
import '../../core/network/api_client.dart';
import '../../core/services/logger_service.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/error_view.dart';
import '../widgets/udi_appbar_logo.dart';

class AuditoriaGuardiasPage extends StatefulWidget {
  const AuditoriaGuardiasPage({super.key});

  @override
  State<AuditoriaGuardiasPage> createState() => _AuditoriaGuardiasPageState();
}

class _AuditoriaGuardiasPageState extends State<AuditoriaGuardiasPage> {
  List<Map<String, dynamic>> _guardias = [];
  bool _cargando = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _cargarGuardias();
  }

  bool _esGuardia(Map<String, dynamic> u) {
    final rol = (u['rol']?.toString() ?? '').toLowerCase();
    return rol.contains('portero');
  }

  Future<void> _cargarGuardias() async {
    setState(() {
      _cargando = true;
      _error = null;
    });
    final api = context.read<ApiClient>();
    try {
      final list = await api.getList('/api/Admin/usuarios');
      if (!mounted) return;
      final usuarios = list.cast<Map<String, dynamic>>();
      setState(() => _guardias = usuarios.where(_esGuardia).toList());
    } catch (e, st) {
      LoggerService.error('Al cargar los guardias', e, st);
      if (!mounted) return;
      setState(() {
        _guardias = [];
        _error = mensajeDeError(e);
      });
    } finally {
      if (mounted) setState(() => _cargando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: AppTheme.udiRed,
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const UdiAppBarLogo(width: 40),
            const SizedBox(width: 8),
            const Text('Auditoría Guardias', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          ],
        ),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh, color: Colors.white),
            onPressed: _cargarGuardias,
          ),
        ],
      ),
      body: _cargando
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? ErrorView(message: _error!, onRetry: _cargarGuardias)
              : _guardias.isEmpty
                  ? const Center(child: Text('No hay guardias registrados'))
                  : RefreshIndicator(
                      onRefresh: _cargarGuardias,
                      child: ListView.builder(
                        padding: const EdgeInsets.all(10),
                        itemCount: _guardias.length,
                        itemBuilder: (context, index) {
                          final u = _guardias[index];
                          return Card(
                            child: ListTile(
                              leading: CircleAvatar(
                                backgroundColor: AppTheme.udiDarkRed,
                                child: Text(
                                  (u['nombreCompleto'] as String? ?? '?').substring(0, 1).toUpperCase(),
                                  style: const TextStyle(color: Colors.white),
                                ),
                              ),
                              title: Text(u['nombreCompleto'] ?? '', style: const TextStyle(fontWeight: FontWeight.bold)),
                              subtitle: Text('${u['documentoIdentidad'] ?? ''}  |  ${u['rol'] ?? ''}'),
                              trailing: const Icon(Icons.chevron_right),
                              onTap: () => Navigator.push(
                                context,
                                MaterialPageRoute(
                                  builder: (context) => AuditoriaGuardiaDetallePage(guardia: u, guardiaId: u['id'] as int),
                                ),
                              ),
                            ),
                          );
                        },
                      ),
                    ),
    );
  }
}

class AuditoriaGuardiaDetallePage extends StatefulWidget {
  final Map<String, dynamic> guardia;
  final int guardiaId;

  const AuditoriaGuardiaDetallePage({super.key, required this.guardia, required this.guardiaId});

  @override
  State<AuditoriaGuardiaDetallePage> createState() => _AuditoriaGuardiaDetallePageState();
}

class _AuditoriaGuardiaDetallePageState extends State<AuditoriaGuardiaDetallePage> {
  DateTime? _fecha;
  Map<String, dynamic>? _datos;
  bool _cargando = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _cargarAprobaciones();
  }

  Future<void> _cargarAprobaciones() async {
    setState(() {
      _cargando = true;
      _error = null;
    });
    final api = context.read<ApiClient>();
    var endpoint = '/api/Admin/usuarios/${widget.guardiaId}/aprobaciones';
    if (_fecha != null) {
      final f = _fecha!;
      endpoint += '?fecha=${f.year.toString().padLeft(4, '0')}-${f.month.toString().padLeft(2, '0')}-${f.day.toString().padLeft(2, '0')}';
    }
    try {
      final data = await api.get(endpoint);
      if (!mounted) return;
      setState(() => _datos = data);
    } catch (e, st) {
      LoggerService.error('Al cargar las aprobaciones del guardia', e, st);
      if (!mounted) return;
      setState(() {
        _datos = null;
        _error = mensajeDeError(e);
      });
    } finally {
      if (mounted) setState(() => _cargando = false);
    }
  }

  Future<void> _elegirDia() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _fecha ?? DateTime.now(),
      firstDate: DateTime(2020),
      lastDate: DateTime.now().add(const Duration(days: 1)),
    );
    if (picked == null) return;
    setState(() => _fecha = picked);
    _cargarAprobaciones();
  }

  @override
  Widget build(BuildContext context) {
    final datos = _datos;
    return Scaffold(
      appBar: AppBar(
        backgroundColor: AppTheme.udiRed,
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const UdiAppBarLogo(width: 40),
            const SizedBox(width: 8),
            Flexible(
              child: Text(widget.guardia['nombreCompleto'] ?? 'Auditoría',
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
            ),
          ],
        ),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.calendar_month, color: Colors.white),
            onPressed: _elegirDia,
          ),
          IconButton(
            icon: const Icon(Icons.refresh, color: Colors.white),
            onPressed: _cargarAprobaciones,
          ),
        ],
      ),
      body: _cargando
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? ErrorView(message: _error!, onRetry: _cargarAprobaciones)
              : datos == null
                  ? const Center(child: Text('Sin datos'))
                  : ListView(
                      padding: const EdgeInsets.all(10),
                      children: [
                        Text(
                          _fecha == null
                              ? 'Todos los días'
                              : 'Día: ${_fecha!.year.toString().padLeft(4, '0')}-${_fecha!.month.toString().padLeft(2, '0')}-${_fecha!.day.toString().padLeft(2, '0')}',
                          style: const TextStyle(fontWeight: FontWeight.bold),
                        ),
                        const SizedBox(height: 8),
                        Row(
                          children: [
                            Expanded(
                              child: _ResumenChip(
                                label: 'Completadas',
                                value: datos['totalCompletadas']?.toString() ?? '0',
                                color: Colors.green,
                              ),
                            ),
                            const SizedBox(width: 10),
                            Expanded(
                              child: _ResumenChip(
                                label: 'Pendientes',
                                value: datos['totalPendientes']?.toString() ?? '0',
                                color: Colors.orange,
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 10),
                        if ((datos['porteria'] as List?)?.isNotEmpty ?? false) ...[
                          const _SeccionHeader(titulo: 'PORTERÍA'),
                          ...(datos['porteria'] as List).map(
                            (item) => _AprobacionCard(
                              titulo: item['visitanteNombre'] ?? '',
                              detalle: 'Doc: ${item['visitanteDocumento'] ?? '-'}\n'
                                  'Entrada: ${_fmt(item['fechaEntrada'])}'
                                  '${item['fechaSalida'] != null ? '\nSalida: ${_fmt(item['fechaSalida'])}' : ''}'
                                  '\nPuerta: ${item['puertaEntrada'] ?? '-'} | Motivo: ${item['motivoVisita'] ?? '-'} | Área: ${item['areaDestino'] ?? '-'}',
                              completada: item['estaCompletada'] == true,
                            ),
                          ),
                          const SizedBox(height: 10),
                        ],
                        if ((datos['parqueo'] as List?)?.isNotEmpty ?? false) ...[
                          const _SeccionHeader(titulo: 'PARQUEO'),
                          ...(datos['parqueo'] as List).map(
                            (item) => _AprobacionCard(
                              titulo: '${item['matricula'] ?? '-'} - ${item['visitanteNombre'] ?? ''}',
                              detalle: 'Doc: ${item['visitanteDocumento'] ?? '-'}\n'
                                  'Ingreso: ${_fmt(item['fechaIngreso'])}'
                                  '${item['fechaSalida'] != null ? '\nSalida: ${_fmt(item['fechaSalida'])}' : ''}'
                                  '\nPuerta: ${item['puertaAcceso'] ?? '-'} | ${item['marca'] ?? ''} ${item['modelo'] ?? ''}',
                              completada: item['estaCompletada'] == true,
                            ),
                          ),
                        ],
                        if ((datos['porteria'] as List?)?.isEmpty ?? true)
                          if ((datos['parqueo'] as List?)?.isEmpty ?? true)
                            const Padding(
                              padding: EdgeInsets.all(24),
                              child: Center(child: Text('Sin aprobaciones en el período')),
                            ),
                      ],
                    ),
    );
  }
}

String _fmt(Object? v) {
  final s = v?.toString();
  return s != null && s.length >= 19 ? s.substring(0, 19) : (s ?? '');
}

class _SeccionHeader extends StatelessWidget {
  final String titulo;

  const _SeccionHeader({required this.titulo});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 6),
      child: Text(
        titulo,
        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: AppTheme.udiDarkRed),
      ),
    );
  }
}

class _ResumenChip extends StatelessWidget {
  final String label;
  final String value;
  final Color color;

  const _ResumenChip({required this.label, required this.value, required this.color});

  @override
  Widget build(BuildContext context) {
    return Card(
      color: color.withValues(alpha: 0.12),
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 10),
        child: Column(
          children: [
            Text(value, style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: color)),
            Text(label, style: const TextStyle(fontWeight: FontWeight.w500)),
          ],
        ),
      ),
    );
  }
}

class _AprobacionCard extends StatelessWidget {
  final String titulo;
  final String detalle;
  final bool completada;

  const _AprobacionCard({required this.titulo, required this.detalle, required this.completada});

  @override
  Widget build(BuildContext context) {
    return Card(
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: completada ? Colors.green : Colors.orange,
          child: Icon(completada ? Icons.check_circle : Icons.hourglass_empty, color: Colors.white),
        ),
        title: Text(titulo, style: const TextStyle(fontWeight: FontWeight.bold)),
        subtitle: Text(detalle),
        isThreeLine: true,
        trailing: Text(
          completada ? 'Completada' : 'Pendiente',
          style: TextStyle(fontWeight: FontWeight.bold, color: completada ? Colors.green : Colors.orange),
        ),
      ),
    );
  }
}