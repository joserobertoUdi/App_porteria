import 'package:flutter/material.dart';
//importamos el tema personalizado que creamos para la app (app_theme.dart)
import 'core/theme/app_theme.dart';
//importamos la pantalla de login que creamos (login_page.dart)
import 'presentation/pages/login_page.dart';

void main() {
  runApp(const UdiAccesApp());
}

class UdiAccesApp extends StatelessWidget {
  const UdiAccesApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Control de Acceso UDI',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.theme, //usamos el tema personalizado en toda la app
      home: const LoginPage(), //la pantalla de inicio es el login
    );
  }
}
