import 'package:flutter/material.dart';
import '../../core/theme/app_theme.dart';
import '../pages/home_page.dart'; // Importamos la página de Home para navegar después del login


class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  // Variable para controlar si la contraseña se ve o no
  bool _isObscure = true;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      // Scaffold es el lienzo en blanco de nuestra pantalla
      body: Container(
        // Aplicamos el degradado rojo que configuramos en el Theme
        decoration: const BoxDecoration(
          gradient: AppTheme.udiGradient,
        ),
        child: Center(
          // SingleChildScrollView evita que la pantalla tire error si el teclado del celular/tablet tapa el formulario
          child: SingleChildScrollView(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 30.0),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  // Aquí iría el Logo de la UDI
                  // Por ahora pondremos un icono de escudo como placeholder hasta que pongamos la imagen real
                  const Icon(
                    Icons.shield,
                    size: 100,
                    color: Colors.white,
                  ),
                  const SizedBox(height: 20),
                  
                  // Título INGRESAR
                  const Text(
                    'INGRESAR',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 28,
                      fontWeight: FontWeight.w900,
                      letterSpacing: 2.0,
                    ),
                  ),
                  const SizedBox(height: 30),

                  // Contenedor del Formulario (El cuadro con borde blanco)
                  Container(
                    padding: const EdgeInsets.all(25.0),
                    decoration: BoxDecoration(
                      color: Colors.transparent,
                      borderRadius: BorderRadius.circular(15.0),
                      border: Border.all(color: Colors.white, width: 1.5),
                    ),
                    child: Column(
                      children: [
                        // Input: Número de registro
                        TextFormField(
                          style: const TextStyle(color: Colors.white),
                          cursorColor: Colors.white,
                          decoration: const InputDecoration(
                            labelText: 'Número de registro',
                            prefixIcon: Icon(Icons.person),
                            // El hintText es el número de ejemplo que se veía en la foto
                            hintText: '12885406', 
                            hintStyle: TextStyle(color: Colors.white70),
                          ),
                        ),
                        const SizedBox(height: 20),

                        // Input: Contraseña
                        TextFormField(
                          obscureText: _isObscure, // Oculta o muestra el texto
                          style: const TextStyle(color: Colors.white),
                          cursorColor: Colors.white,
                          decoration: InputDecoration(
                            labelText: 'Contraseña',
                            prefixIcon: const Icon(Icons.fingerprint),
                            // Botón del ojito para alternar la visibilidad
                            suffixIcon: IconButton(
                              icon: Icon(
                                _isObscure ? Icons.visibility_off : Icons.visibility,
                                color: Colors.white,
                              ),
                              onPressed: () {
                                setState(() {
                                  _isObscure = !_isObscure; // Cambia el estado
                                });
                              },
                            ),
                          ),
                        ),
                        const SizedBox(height: 30),

                        // Botón de INICIAR SESIÓN
                        SizedBox(
                          width: double.infinity, // Hace que el botón ocupe todo el ancho disponible
                          child: ElevatedButton(
                            onPressed: () {
                              // Borra el login del navegador para que no se pueda volver atrás después de iniciar sesión
                              Navigator.pushReplacement(
                                context,
                                MaterialPageRoute(builder: (context) => const HomePage()),
                                );

                            },
                            child: const Text('INICIAR SESIÓN'),
                          ),
                        ),
                        const SizedBox(height: 15),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}