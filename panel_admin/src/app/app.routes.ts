import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/auth.guard';
import { LayoutComponent } from './presentation/pages/layout/layout.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./presentation/pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'historial' },
      {
        path: 'historial',
        loadComponent: () =>
          import('./presentation/pages/historial/historial.component').then((m) => m.HistorialComponent),
      },
      {
        path: 'usuarios',
        loadComponent: () =>
          import('./presentation/pages/usuarios/usuarios.component').then((m) => m.UsuariosComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
