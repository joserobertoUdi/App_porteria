import 'package:flutter/material.dart';
import '../../core/theme/app_theme.dart';

class RegistrarSalidaPage extends StatefulWidget {
  const RegistrarSalidaPage({super.key});

  @override
  State<RegistrarSalidaPage> createState() => _RegistrarSalidaPageState();
}

class _RegistrarSalidaPageState extends State<RegistrarSalidaPage> {
  final TextEditingController _searchController = TextEditingController();
  final TextEditingController _nombreController = TextEditingController();
  final TextEditingController _ciController = TextEditingController();
  final TextEditingController _horaController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _obtenerHoraActual();
  }

  void _obtenerHoraActual() {
    final ahora = DateTime.now();
    final horaStr = ahora.hour.toString().padLeft(2, '0');
    final minutoStr = ahora.minute.toString().padLeft(2, '0');
    _horaController.text = '$horaStr:$minutoStr';
  }

  // 
  void _buscarRegistro() {
    // Ocultar el teclado del celular al buscar
    FocusScope.of(context).unfocus();

    final ciBuscado = _searchController.text.trim();
    
    // Si el guardia le da a buscar sin escribir nada, le avisamos
    if (ciBuscado.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Por favor, escriba un C.I. primero'),
          backgroundColor: Colors.redAccent,
        ),
      );
      return;
    }

    // Simulación: Encontramos al estudiante
    if (ciBuscado == '123') {
      setState(() {
        _nombreController.text = 'Jefferson (Estudiante de Sistemas)';
        _ciController.text = ciBuscado;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Registro encontrado. Verifique y confirme.'),
          backgroundColor: Colors.green,
        ),
      );
    } 
    // Simulación: No lo encontramos (Registro manual)
    else {
      setState(() {
        _nombreController.clear();
        _ciController.text = ciBuscado;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Sin registro previo. Ingrese el nombre manualmente.'),
          backgroundColor: Colors.orange,
        ),
      );
    }
  }

  @override
  void dispose() {
    _searchController.dispose();
    _nombreController.dispose();
    _ciController.dispose();
    _horaController.dispose();
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
          'Registrar Salida',
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
                const Text(
                  'Buscar registro para marcar salida',
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
                        textInputAction: TextInputAction.search,
                        onSubmitted: (_) => _buscarRegistro(), 
                        decoration: InputDecoration(
                          hintText: 'Ingrese C.I...',
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
                      // El botón ahora tiene el icono de buscar para que sea más obvio
                      onPressed: _buscarRegistro, 
                      child: const Icon(Icons.person_search),
                    ),
                  ],
                ),
                
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 20.0),
                  child: Divider(color: Colors.white54),
                ),

                const Text(
                  'Confirmación de Datos',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white),
                ),
                const SizedBox(height: 15),

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

                Row(
                  children: [
                    Expanded(
                      flex: 6,
                      child: TextField(
                        controller: _ciController,
                        keyboardType: TextInputType.number,
                        style: const TextStyle(color: Colors.white),
                        cursorColor: Colors.white,
                        decoration: InputDecoration(
                          labelText: 'Nº de C.I.',
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
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      flex: 4,
                      child: TextField(
                        controller: _horaController,
                        readOnly: true,
                        style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                        decoration: InputDecoration(
                          labelText: 'Hora Salida',
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
                const SizedBox(height: 30),

                // Botón Principal: Confirmar Salida
                ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.white,
                    foregroundColor: AppTheme.udiRed,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30.0)),
                  ),
                  onPressed: () {
                    // Ocultar el teclado por si acaso
                    FocusScope.of(context).unfocus();

                    // Mostrar mensaje de éxito verde al guardia
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(
                        content: Text(' ¡Salida registrada exitosamente!'),
                        backgroundColor: Colors.green,
                        duration: Duration(seconds: 2), // El cartel dura 2 segundos
                      ),
                    );

                    // Imprimir en consola para ti
                    debugPrint('Salida registrada: ${_nombreController.text} a las ${_horaController.text}');

                    // Esperar 1.5 segundos para que el guardia lea el mensaje y luego volver al menú
                    Future.delayed(const Duration(milliseconds: 1500), () {
                      Navigator.pop(context);
                    });
                  },
                  child: const Text(
                    'CONFIRMAR SALIDA',
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