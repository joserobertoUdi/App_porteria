import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'udi-empty',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="udi-empty">
      <span class="udi-empty__icono" aria-hidden="true">∅</span>
      <p class="udi-empty__texto">{{ mensaje() }}</p>
    </div>
  `,
  styles: `
    .udi-empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 10px;
      padding: 48px 16px;
      color: #9e9e9e;
      text-align: center;
    }
    .udi-empty__icono {
      font-size: 2.4rem;
      line-height: 1;
    }
    .udi-empty__texto {
      margin: 0;
      font-size: 0.95rem;
      font-weight: 600;
    }
  `,
})
export class UdiEmptyComponent {
  readonly mensaje = input<string>('Sin registros');
}