import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme/app_theme.dart';
import '../../presentation/providers/auth_provider.dart';
import 'home_page.dart';
import 'admin_page.dart';

/// Pantalla de inicio de sesión.
///
/// Funcionalidades:
/// - Pre-llenado del DNI con el último usuario que inició sesión.
/// - Login con contraseña (método principal).
/// - Login biométrico (huella / Face ID) cuando el dispositivo lo soporta
///   y el usuario lo ha activado previamente.
class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _dniController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _isObscure = true;
  @override
  void initState() {
    super.initState();
    _loadLastDni();
  }

  /// Carga el DNI guardado y pre-llena el campo de texto.
  ///
  /// Si la biometría está habilitada y hay credenciales cacheadas,
  /// lanza automáticamente el prompt biométrico.
  Future<void> _loadLastDni() async {
    // Pequeña espera para que el provider termine de cargar preferencias.
    await Future<void>.delayed(Duration.zero);
    if (!mounted) return;
    final auth = context.read<AuthProvider>();
    final lastDni = auth.lastDni;
    if (lastDni != null && lastDni.isNotEmpty && mounted) {
      _dniController.text = lastDni;
    }

    // Auto-lanzar biometría si está disponible y hay credenciales.
    if (auth.canUseBiometric && mounted) {
      // Espera a que el primer frame se renderice.
      await Future<void>.delayed(const Duration(milliseconds: 300));
      if (!mounted) return;
      _loginWithBiometric();
    }
  }

  @override
  void dispose() {
    _dniController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  // ── Login con contraseña ──────────────────────────────────────────────

  Future<void> _login() async {
    final dni = _dniController.text.trim();
    final password = _passwordController.text;

    if (dni.isEmpty || password.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Ingrese carnet y contraseña')),
      );
      return;
    }

    final auth = context.read<AuthProvider>();
    final ok = await auth.login(dni, password);

    if (!mounted) return;

    if (ok) {
      _navigateToHome(auth);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(auth.error ?? 'Credenciales inválidas')),
      );
    }
  }

  // ── Login biométrico ──────────────────────────────────────────────────

  Future<void> _loginWithBiometric() async {
    final auth = context.read<AuthProvider>();
    final ok = await auth.loginWithBiometric();

    if (!mounted) return;

    if (ok) {
      _navigateToHome(auth);
    } else {
      final msg = auth.error ?? 'No se pudo autenticar con biometría';
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(msg)),
      );
    }
  }

  // ── Navegación post-login ─────────────────────────────────────────────

  void _navigateToHome(AuthProvider auth) {
    if (auth.esAdministrador) {
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(builder: (context) => const AdminPage()),
      );
    } else {
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(builder: (context) => const HomePage()),
      );
    }
  }

  // ── UI ────────────────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(gradient: AppTheme.udiGradient),
        child: Center(
          child: SingleChildScrollView(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 30.0),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  // ── Logo ──
                  Image.asset(
                    'assets/branding/logo-udi-blanco.png',
                    width: 160,
                    height: 90,
                    fit: BoxFit.contain,
                    semanticLabel:
                        'Universidad para el Desarrollo y la Innovación',
                  ),
                  const SizedBox(height: 12),
                  const Text(
                    'CONTROL DE ACCESO',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 24,
                      fontWeight: FontWeight.w900,
                      letterSpacing: 2.0,
                    ),
                  ),
                  const SizedBox(height: 30),

                  // ── Formulario ──
                  Container(
                    padding: const EdgeInsets.all(25.0),
                    decoration: BoxDecoration(
                      color: Colors.transparent,
                      borderRadius: BorderRadius.circular(15.0),
                      border: Border.all(color: Colors.white, width: 1.5),
                    ),
                    child: Consumer<AuthProvider>(
                      builder: (context, auth, _) {
                        return Column(
                          children: [
                            // ── Campo DNI (pre-llenado si hay último usuario) ──
                            TextFormField(
                              controller: _dniController,
                              style: const TextStyle(color: Colors.white),
                              cursorColor: Colors.white,
                              decoration: const InputDecoration(
                                labelText: 'Usuario',
                                prefixIcon: Icon(Icons.person),
                              ),
                            ),
                            const SizedBox(height: 20),

                            // ── Campo contraseña ──
                            TextFormField(
                              controller: _passwordController,
                              obscureText: _isObscure,
                              style: const TextStyle(color: Colors.white),
                              cursorColor: Colors.white,
                              decoration: InputDecoration(
                                labelText: 'Contraseña',
                                prefixIcon: _buildBiometricIcon(auth),
                                suffixIcon: IconButton(
                                  icon: Icon(
                                    _isObscure
                                        ? Icons.visibility_off
                                        : Icons.visibility,
                                    color: Colors.white,
                                  ),
                                  onPressed: () =>
                                      setState(() => _isObscure = !_isObscure),
                                ),
                              ),
                            ),
                            const SizedBox(height: 30),

                            // ── Botón ingresar ──
                            SizedBox(
                              width: double.infinity,
                              child: ElevatedButton(
                                onPressed: auth.loading ? null : _login,
                                child: auth.loading
                                    ? const SizedBox(
                                        width: 24,
                                        height: 24,
                                        child: CircularProgressIndicator(
                                            strokeWidth: 2),
                                      )
                                    : const Text('INICIAR SESIÓN'),
                              ),
                            ),

                            // ── Botón biométrico (solo si está disponible) ──
                            if (auth.canUseBiometric) ...[
                              const SizedBox(height: 16),
                              _buildBiometricButton(auth),
                            ],
                          ],
                        );
                      },
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

  /// Ícono de huella en el campo de contraseña.
  ///
  /// Si la biometría está disponible y activada, muestra el ícono de huella
  /// como prefixIcon y al tocarlo ejecuta el login biométrico.
  /// Si no, muestra un ícono de candado genérico.
  Widget? _buildBiometricIcon(AuthProvider auth) {
    if (!auth.canUseBiometric) {
      return const Icon(Icons.lock);
    }
    return IconButton(
      icon: const Icon(Icons.fingerprint, color: Colors.white),
      tooltip: 'Iniciar sesión con huella',
      onPressed: auth.loading ? null : _loginWithBiometric,
    );
  }

  /// Botón grande de acceso biométrico debajo del botón principal.
  Widget _buildBiometricButton(AuthProvider auth) {
    return SizedBox(
      width: double.infinity,
      child: OutlinedButton.icon(
        onPressed: auth.loading ? null : _loginWithBiometric,
        icon: const Icon(Icons.fingerprint, size: 28),
        label: const Text(
          'ACCEDER CON HUELLA',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
        style: OutlinedButton.styleFrom(
          foregroundColor: Colors.white,
          side: const BorderSide(color: Colors.white, width: 1.5),
          padding: const EdgeInsets.symmetric(vertical: 14),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(30),
          ),
        ),
      ),
    );
  }
}
