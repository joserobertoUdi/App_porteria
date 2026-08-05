import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { UdiToastComponent } from '../../../shared/components/udi-toast/udi-toast.component';
import { AuthSessionService } from '../../services/auth-session.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, UdiToastComponent],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutComponent {
  private readonly session = inject(AuthSessionService);

  readonly sesion = this.session.sesion;
  readonly inicial = computed<string>(() => this.sesion()?.nombreCompleto?.charAt(0)?.toUpperCase() ?? 'U');

  cerrarSesion(): void {
    this.session.logout();
  }
}