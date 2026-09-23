import 'package:flutter/material.dart';
import '../../core/theme/app_theme.dart';
import '../widgets/custom_action_card.dart'; 
import 'registrar_entrada_page.dart';
import 'registrar_salida_page.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[100], 
      appBar: AppBar(
        backgroundColor: AppTheme.udiRed,
        title: const Text(
          'Panel de Control - UDI',
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.white),
            onPressed: () {
              Navigator.pushReplacementNamed(context, '/'); 
            },
          )
        ],
      ),
      // Envolvemos el contenido en un Center
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(20.0),
          child: GridView.count(
            // shrinkWrap hace que la grilla ocupe solo el espacio necesario para sus 2 botones
            shrinkWrap: true, 
            physics: const NeverScrollableScrollPhysics(), 
            crossAxisCount: 2, 
            crossAxisSpacing: 20, 
            mainAxisSpacing: 20, 
            children: [
              CustomActionCard(
                title: 'REGISTRAR\nENTRADA',
                icon: Icons.login_rounded,
                onTap: () {
                // Navegar a la pantalla de Registrar Entrada
              Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const RegistrarEntradaPage()),
                    );
                  },
                ),
              CustomActionCard(
                title: 'REGISTRAR\nSALIDA',
                icon: Icons.logout_rounded,
                onTap: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(builder: (context) => const RegistrarSalidaPage()),
                  );
                },
              ),
            ],
          ),
        ),
      ),
    );
  }
}