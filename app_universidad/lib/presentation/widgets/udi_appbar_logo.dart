import 'package:flutter/material.dart';

/// Identidad visual compacta para barras superiores en pantallas móviles.
class UdiAppBarLogo extends StatelessWidget {
  const UdiAppBarLogo({super.key, this.width = 58});

  final double width;

  @override
  Widget build(BuildContext context) {
    return ExcludeSemantics(
      child: Padding(
        padding: const EdgeInsets.only(left: 4, right: 2),
        child: Center(
          child: Image.asset(
            'assets/branding/logo-udi-blanco.png',
            width: width,
            height: 30,
            fit: BoxFit.contain,
            filterQuality: FilterQuality.medium,
          ),
        ),
      ),
    );
  }
}
