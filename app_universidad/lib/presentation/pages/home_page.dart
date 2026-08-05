import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme/app_theme.dart';
import '../providers/auth_provider.dart';
import '../widgets/custom_action_card.dart';
import '../widgets/udi_appbar_logo.dart';
import 'admin_page.dart';
import 'registrar_entrada_page.dart';
import 'registrar_salida_page.dart';
import 'registrar_entrada_parqueo_page.dart';
import 'registrar_salida_parqueo_page.dart';
import 'parqueos_activos_page.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();

    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        backgroundColor: AppTheme.udiRed,
        title: const Text(
          'Control UDI',
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
        actions: [
          const UdiAppBarLogo(width: 52),
          Padding(
            padding: const EdgeInsets.only(right: 8),
            child: Center(
              child: Text(
                auth.nombreCompleto,
                style: const TextStyle(color: Colors.white70, fontSize: 12),
              ),
            ),
          ),
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.white),
            onPressed: () {
              auth.logout();
              Navigator.of(context).pushNamedAndRemoveUntil('/', (route) => false);
            },
          ),
        ],
      ),
      body: LayoutBuilder(
        builder: (context, constraints) {
          final available = constraints.maxWidth > 520 ? 520.0 : constraints.maxWidth;
          final isNarrow = available < 380;
          final hPadding = isNarrow ? 12.0 : 20.0;
          final spacing = isNarrow ? 12.0 : 20.0;
          final cellWidth = (available - hPadding * 2 - spacing) / 2;
          final mainAxisExtent = cellWidth * 1.15;

          final cards = <Widget>[
            if (auth.esAdministrador || auth.esPorteroPorteria)
              CustomActionCard(
                title: 'REGISTRAR\nENTRADA',
                icon: Icons.login_rounded,
                onTap: () => Navigator.push(
                  context,
                  MaterialPageRoute(builder: (context) => const RegistrarEntradaPage()),
                ),
              ),
            if (auth.esAdministrador || auth.esPorteroPorteria)
              CustomActionCard(
                title: 'REGISTRAR\nSALIDA',
                icon: Icons.logout_rounded,
                onTap: () => Navigator.push(
                  context,
                  MaterialPageRoute(builder: (context) => const RegistrarSalidaPage()),
                ),
              ),
            if (auth.esAdministrador || auth.esPorteroParqueo)
              CustomActionCard(
                title: 'ENTRADA\nPARQUEO',
                icon: Icons.directions_car,
                onTap: () => Navigator.push(
                  context,
                  MaterialPageRoute(builder: (context) => const RegistrarEntradaParqueoPage()),
                ),
              ),
            if (auth.esAdministrador || auth.esPorteroParqueo)
              CustomActionCard(
                title: 'SALIDA\nPARQUEO',
                icon: Icons.directions_car,
                onTap: () => Navigator.push(
                  context,
                  MaterialPageRoute(builder: (context) => const RegistrarSalidaParqueoPage()),
                ),
              ),
            if (auth.esAdministrador || auth.esPorteroParqueo)
              CustomActionCard(
                title: 'PARQUEOS\nACTIVOS',
                icon: Icons.list_alt_rounded,
                onTap: () => Navigator.push(
                  context,
                  MaterialPageRoute(builder: (context) => const ParqueosActivosPage()),
                ),
              ),
            if (auth.esAdministrador)
              CustomActionCard(
                title: 'ADMIN',
                icon: Icons.admin_panel_settings,
                onTap: () => Navigator.push(
                  context,
                  MaterialPageRoute(builder: (context) => const AdminPage()),
                ),
              ),
          ];

          return Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 520),
              child: SingleChildScrollView(
                padding: EdgeInsets.all(hPadding),
                child: GridView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    crossAxisSpacing: spacing,
                    mainAxisSpacing: spacing,
                    mainAxisExtent: mainAxisExtent,
                  ),
                  itemCount: cards.length,
                  itemBuilder: (context, index) => cards[index],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
