import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme/app_theme.dart';
import '../providers/auth_provider.dart';
import '../widgets/custom_action_card.dart';
import '../widgets/udi_appbar_logo.dart';
import 'admin_usuarios_page.dart';
import 'historial_porteria_page.dart';
import 'historial_parqueo_page.dart';

class AdminPage extends StatelessWidget {
  const AdminPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        backgroundColor: AppTheme.udiRed,
        title: const Text(
          'Administración',
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
        actions: [
          const UdiAppBarLogo(),
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.white),
            onPressed: () {
              context.read<AuthProvider>().logout();
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
            CustomActionCard(
              title: 'USUARIOS',
              icon: Icons.people,
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const AdminUsuariosPage()),
              ),
            ),
            CustomActionCard(
              title: 'HISTORIAL\nPORTERÍA',
              icon: Icons.history,
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const HistorialPorteriaPage()),
              ),
            ),
            CustomActionCard(
              title: 'HISTORIAL\nPARQUEO',
              icon: Icons.directions_car,
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const HistorialParqueoPage()),
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
