import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'udi-loading',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="udi-loading" role="status" aria-label="Cargando">
      <span class="udi-loading__spinner"></span>
      <p class="udi-loading__texto">{{ mensaje() }}</p>
    </div>
  `,
  styles: `
    .udi-loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 12px;
      padding: 40px 16px;
      color: var(--udi-rojo);
    }
    .udi-loading__spinner {
      width: 44px;
      height: 44px;
      border: 4px solid rgba(213, 0, 0, 0.2);
      border-top-color: var(--udi-rojo);
      border-radius: 50%;
      animation: udi-girar 0.8s linear infinite;
    }
    .udi-loading__texto {
      margin: 0;
      font-size: 0.9rem;
      font-weight: 600;
    }
    @keyframes udi-girar {
      to {
        transform: rotate(360deg);
      }
    }
  `,
})
export class UdiLoadingComponent {
  readonly mensaje = input<string>('Cargando…');
}