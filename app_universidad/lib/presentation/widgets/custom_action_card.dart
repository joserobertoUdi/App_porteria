import 'package:flutter/material.dart';
import '../../core/theme/app_theme.dart';

class CustomActionCard extends StatelessWidget {
  final String title;
  final IconData icon;
  final VoidCallback onTap;

  const CustomActionCard({
    super.key,
    required this.title,
    required this.icon,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final width = constraints.maxWidth;
        final scale = (width / 180).clamp(0.7, 1.0);
        final iconSize = (60 * scale).clamp(40.0, 60.0);
        final titleFont = (18 * scale).clamp(13.0, 18.0);
        final gap = 15.0 * scale;
        final padding = (25 * scale).clamp(12.0, 25.0);

        return InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(15.0),
          child: Container(
            width: double.infinity,
            height: double.infinity,
            padding: EdgeInsets.all(padding),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(15.0),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.1),
                  blurRadius: 10,
                  offset: const Offset(0, 5),
                ),
              ],
            ),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(icon, size: iconSize, color: AppTheme.udiRed),
                SizedBox(height: gap),
                Expanded(
                  child: FittedBox(
                    fit: BoxFit.scaleDown,
                    child: Text(
                      title,
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: AppTheme.udiDarkRed,
                        fontSize: titleFont,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}
