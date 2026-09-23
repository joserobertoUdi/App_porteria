import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { AuthRepositoryImpl } from './data/repositories/auth.repository.impl';
import { AdminRepositoryImpl } from './data/repositories/admin.repository.impl';
import { AUTH_REPOSITORY } from './domain/repositories/auth.repository';
import { ADMIN_REPOSITORY } from './domain/repositories/admin.repository';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: AUTH_REPOSITORY, useClass: AuthRepositoryImpl },
    { provide: ADMIN_REPOSITORY, useClass: AdminRepositoryImpl }
  ]
};
