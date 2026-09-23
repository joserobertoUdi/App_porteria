import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'core/constants/sharepoint_constants.dart';
import 'core/network/api_client.dart';
import 'core/services/biometric_service.dart';
import 'core/theme/app_theme.dart';
import 'data/datasources/auth_remote_data_source.dart';
import 'data/datasources/parqueo_remote_data_source.dart';
import 'data/datasources/sistema_remote_data_source.dart';
import 'data/repositories/parqueo_repository_impl.dart';
import 'domain/repositories/parqueo_repository.dart';
import 'presentation/pages/login_page.dart';
import 'presentation/providers/auth_provider.dart';

void main() {
  if (!SharepointConstants.isConfigured) {
    debugPrint(
      '⚠️  PROVIDER_KEY no configurada. Las fotos de portería no se '
      'podrán descargar. Ejecute con: '
      '--dart-define=PROVIDER_KEY=tu_clave',
    );
  }
  runApp(const UdiAccesApp());
}

class UdiAccesApp extends StatefulWidget {
  const UdiAccesApp({super.key});

  @override
  State<UdiAccesApp> createState() => _UdiAccesAppState();
}

class _UdiAccesAppState extends State<UdiAccesApp> {
  final _navigatorKey = GlobalKey<NavigatorState>();

  @override
  Widget build(BuildContext context) {
    final apiClient = ApiClient();
    final authDataSource = AuthRemoteDataSource(apiClient);
    final sistemaDataSource = SistemaRemoteDataSource(apiClient);
    final biometricService = BiometricService();

    return MultiProvider(
      providers: [
        ChangeNotifierProvider(
            create: (_) => AuthProvider(
                  apiClient,
                  authDataSource,
                  sistemaDataSource,
                  navigatorKey: _navigatorKey,
                  biometricService: biometricService,
                )),
        Provider(create: (_) => apiClient),
        Provider(create: (_) => sistemaDataSource),
        Provider(create: (_) => ParqueoRemoteDataSource(apiClient)),
        Provider<ParqueoRepository>(create: (_) => ParqueoRepositoryImpl(ParqueoRemoteDataSource(apiClient))),
      ],
      child: MaterialApp(
        title: 'Control de Acceso UDI',
        debugShowCheckedModeBanner: false,
        navigatorKey: _navigatorKey,
        theme: AppTheme.theme,
        initialRoute: '/',
        routes: {
          '/': (context) => const LoginPage(),
        },
      ),
    );
  }
}
