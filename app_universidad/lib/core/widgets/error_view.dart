import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

/// Widget reutilizable para mostrar errores con opción de reintentar.
///
/// Uso:
/// ```dart
/// if (_error != null)
///   ErrorView(
///     message: _error!,
///     onRetry: _cargarDatos,
///   )
/// ```
class ErrorView extends StatelessWidget {
  const ErrorView({
    super.key,
    required this.message,
    this.onRetry,
    this.icon = Icons.error_outline,
  });

  /// Mensaje de error comprensible para el usuario.
  final String message;

  /// Callback al presionar "Reintentar". Si es `null`, no se muestra el botón.
  final VoidCallback? onRetry;

  /// Ícono a mostrar. Por defecto `Icons.error_outline`.
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 56, color: AppTheme.udiRed.withOpacity(0.7)),
            const SizedBox(height: 16),
            Text(
              message,
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 15, color: Colors.black54),
            ),
            if (onRetry != null) ...[
              const SizedBox(height: 20),
              ElevatedButton.icon(
                onPressed: onRetry,
                icon: const Icon(Icons.refresh),
                label: const Text('REINTENTAR'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.udiRed,
                  foregroundColor: Colors.white,
                  padding:
                      const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(30)),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
