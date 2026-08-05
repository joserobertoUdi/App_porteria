import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme/app_theme.dart';
import '../../presentation/providers/auth_provider.dart';
import 'home_page.dart';
import 'admin_page.dart';

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
  void dispose() {
    _dniController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

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
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(auth.error ?? 'Credenciales inválidas')),
      );
    }
  }

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
                  Image.asset(
                    'assets/branding/logo-udi-blanco.png',
                    width: 190,
                    height: 110,
                    fit: BoxFit.contain,
                    semanticLabel: 'Universidad para el Desarrollo y la Innovación',
                  ),
                  const SizedBox(height: 12),
                  const Text('INGRESAR', style: TextStyle(
                    color: Colors.white, fontSize: 28,
                    fontWeight: FontWeight.w900, letterSpacing: 2.0,
                  )),
                  const SizedBox(height: 30),
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
                            TextFormField(
                              controller: _passwordController,
                              obscureText: _isObscure,
                              style: const TextStyle(color: Colors.white),
                              cursorColor: Colors.white,
                              decoration: InputDecoration(
                                labelText: 'Contraseña',
                                prefixIcon: const Icon(Icons.fingerprint),
                                suffixIcon: IconButton(
                                  icon: Icon(
                                    _isObscure ? Icons.visibility_off : Icons.visibility,
                                    color: Colors.white,
                                  ),
                                  onPressed: () => setState(() => _isObscure = !_isObscure),
                                ),
                              ),
                            ),
                            const SizedBox(height: 30),
                            SizedBox(
                              width: double.infinity,
                              child: ElevatedButton(
                                onPressed: auth.loading ? null : _login,
                                child: auth.loading
                                    ? const SizedBox(
                                        width: 24, height: 24,
                                        child: CircularProgressIndicator(strokeWidth: 2),
                                      )
                                    : const Text('INICIAR SESIÓN'),
                              ),
                            ),
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
}
