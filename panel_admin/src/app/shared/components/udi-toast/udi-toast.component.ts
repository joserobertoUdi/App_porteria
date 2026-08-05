import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'udi-toast',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (toast.actual(); as notificacion) {
      <div
        class="udi-toast"
        [class.udi-toast--exito]="notificacion.tipo === 'exito'"
        [class.udi-toast--error]="notificacion.tipo === 'error'"
        [class.udi-toast--info]="notificacion.tipo === 'info'"
        role="status"
        (click)="toast.ocultar()"
      >
        {{ notificacion.mensaje }}
      </div>
    }
  `,
  styles: `
    .udi-toast {
      position: fixed;
      left: 50%;
      bottom: 24px;
      transform: translateX(-50%);
      max-width: min(92vw, 480px);
      padding: 14px 20px;
      border-radius: 12px;
      color: #fff;
      font-weight: 600;
      font-size: 0.9rem;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.25);
      z-index: 1200;
      cursor: pointer;
      animation: udi-entrar 0.2s ease-out;
    }
    .udi-toast--exito {
      background: #2e7d32;
    }
    .udi-toast--error {
      background: var(--udi-rojo-oscuro);
    }
    .udi-toast--info {
      background: #37474f;
    }
    @keyframes udi-entrar {
      from {
        opacity: 0;
        transform: translate(-50%, 12px);
      }
      to {
        opacity: 1;
        transform: translate(-50%, 0);
      }
    }
  `,
})
export class UdiToastComponent {
  readonly toast = inject(ToastService);
}