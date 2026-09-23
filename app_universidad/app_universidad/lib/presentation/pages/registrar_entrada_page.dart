import 'package:flutter/material.dart';
import '../../core/theme/app_theme.dart';

class RegistrarEntradaPage extends StatefulWidget {
  const RegistrarEntradaPage({super.key});

  @override
  State<RegistrarEntradaPage> createState() => _RegistrarEntradaPageState();
}

class _RegistrarEntradaPageState extends State<RegistrarEntradaPage> {
  final TextEditingController _searchController = TextEditingController();
  final TextEditingController _nombreController = TextEditingController();
  final TextEditingController _ciController = TextEditingController();
  final TextEditingController _motivoController = TextEditingController();
  
  // Controlador para la hora
  final TextEditingController _horaController = TextEditingController(); 

  String _puertaSeleccionada = 'Principal';
  final List<String> _puertas = ['Principal', 'Parqueo'];

  // initState se ejecuta automáticamente apenas se abre la pantalla
  @override
  void initState() {
    super.initState();
    _obtenerHoraActual();
  }

  // Función para capturar y darle formato a la hora (Ejemplo: "14:05")
  void _obtenerHoraActual() {
    final ahora = DateTime.now();
    final horaStr = ahora.hour.toString().padLeft(2, '0');
    final minutoStr = ahora.minute.toString().padLeft(2, '0');
    _horaController.text = '$horaStr:$minutoStr';
  }

  @override
  void dispose() {
    _searchController.dispose();
    _nombreController.dispose();
    _ciController.dispose();
    _motivoController.dispose();
    _horaController.dispose(); // Limpiamos la memoria
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      extendBodyBehindAppBar: true, 
      appBar: AppBar(
        backgroundColor: Colors.transparent, 
        elevation: 0, 
        title: const Text(
          'Registrar Entrada',
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: const BoxDecoration(
          gradient: AppTheme.udiGradient,
        ),
        child: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(20.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // BUSCADOR RÁPIDO
                const Text(
                  'Búsqueda rápida por registro (C.I.)',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white),
                ),
                const SizedBox(height: 10),
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _searchController,
                        keyboardType: TextInputType.number,
                        style: const TextStyle(color: Colors.white), 
                        cursorColor: Colors.white,
                        decoration: InputDecoration(
                          hintText: 'Buscar estudiante/visita...',
                          hintStyle: const TextStyle(color: Colors.white60),
                          prefixIcon: const Icon(Icons.search, color: Colors.white),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10.0),
                            borderSide: const BorderSide(color: Colors.white70),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10.0),
                            borderSide: const BorderSide(color: Colors.white, width: 2.0),
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(width: 10),
                    ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.white, 
                        foregroundColor: AppTheme.udiRed, 
                        padding: const EdgeInsets.symmetric(vertical: 15, horizontal: 15),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10.0)),
                      ),
                      onPressed: () {
                        debugPrint('Buscando: ${_searchController.text}');
                      },
                      child: const Icon(Icons.person_search),
                    ),
                  ],
                ),
                
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 20.0),
                  child: Divider(color: Colors.white54), 
                ),

                // FORMULARIO DE INGRESO
                const Text(
                  'Datos de Ingreso',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white),
                ),
                const SizedBox(height: 15),

                // Campo: Nombre
                TextField(
                  controller: _nombreController,
                  style: const TextStyle(color: Colors.white),
                  cursorColor: Colors.white,
                  decoration: InputDecoration(
                    labelText: 'Nombre Completo',
                    labelStyle: const TextStyle(color: Colors.white70),
                    prefixIcon: const Icon(Icons.person_outline, color: Colors.white),
                    enabledBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(10.0),
                      borderSide: const BorderSide(color: Colors.white70),
                    ),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(10.0),
                      borderSide: const BorderSide(color: Colors.white, width: 2.0),
                    ),
                  ),
                ),
                const SizedBox(height: 15),

                // Campo: C.I. / Registro
                TextField(
                  controller: _ciController,
                  keyboardType: TextInputType.number,
                  style: const TextStyle(color: Colors.white),
                  cursorColor: Colors.white,
                  decoration: InputDecoration(
                    labelText: 'Nº de C.I. o Registro',
                    labelStyle: const TextStyle(color: Colors.white70),
                    prefixIcon: const Icon(Icons.badge_outlined, color: Colors.white),
                    enabledBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(10.0),
                      borderSide: const BorderSide(color: Colors.white70),
                    ),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(10.0),
                      borderSide: const BorderSide(color: Colors.white, width: 2.0),
                    ),
                  ),
                ),
                const SizedBox(height: 15),

                // Campo de Hora Automática y Puerta (Los ponemos en una Row para ahorrar espacio en pantalla)
                Row(
                  children: [
                    // Selector de Puerta
                    Expanded(
                      flex: 6, // Toma un poco más de espacio que la hora
                      child: DropdownButtonFormField<String>(
                        value: _puertaSeleccionada,
                        style: const TextStyle(color: Colors.white), 
                        dropdownColor: AppTheme.udiRed, 
                        iconEnabledColor: Colors.white, 
                        decoration: InputDecoration(
                          labelText: 'Acceso',
                          labelStyle: const TextStyle(color: Colors.white70),
                          prefixIcon: const Icon(Icons.door_front_door_outlined, color: Colors.white),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10.0),
                            borderSide: const BorderSide(color: Colors.white70),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10.0),
                            borderSide: const BorderSide(color: Colors.white, width: 2.0),
                          ),
                        ),
                        items: _puertas.map((String puerta) {
                          return DropdownMenuItem<String>(
                            value: puerta,
                            child: Text(puerta),
                          );
                        }).toList(),
                        onChanged: (String? newValue) {
                          setState(() {
                            _puertaSeleccionada = newValue!;
                          });
                        },
                      ),
                    ),
                    const SizedBox(width: 10),
                    
                    // Campo de Hora Automática (Solo lectura)
                    Expanded(
                      flex: 4,
                      child: TextField(
                        controller: _horaController,
                        readOnly: true, // ¡Clave! El guardia no puede editarlo
                        style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                        decoration: InputDecoration(
                          labelText: 'Hora',
                          labelStyle: const TextStyle(color: Colors.white70),
                          prefixIcon: const Icon(Icons.access_time, color: Colors.white),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10.0),
                            borderSide: const BorderSide(color: Colors.white70),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10.0),
                            borderSide: const BorderSide(color: Colors.transparent),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 15),

                // Campo: Motivo de visita
                TextField(
                  controller: _motivoController,
                  maxLines: 3,
                  style: const TextStyle(color: Colors.white),
                  cursorColor: Colors.white,
                  decoration: InputDecoration(
                    labelText: 'Motivo de la visita',
                    labelStyle: const TextStyle(color: Colors.white70),
                    alignLabelWithHint: true, 
                    prefixIcon: const Icon(Icons.edit_note, color: Colors.white),
                    enabledBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(10.0),
                      borderSide: const BorderSide(color: Colors.white70),
                    ),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(10.0),
                      borderSide: const BorderSide(color: Colors.white, width: 2.0),
                    ),
                  ),
                ),
                const SizedBox(height: 30),

                ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.white, 
                    foregroundColor: AppTheme.udiRed, 
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30.0)),
                  ),
                  onPressed: () {
                    debugPrint('Ingreso: ${_nombreController.text} a las ${_horaController.text}');
                  },
                  child: const Text(
                    'CONFIRMAR ENTRADA',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}