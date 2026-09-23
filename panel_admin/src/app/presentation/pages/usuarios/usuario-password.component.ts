import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { UsuarioLista } from '../../../domain/models/usuario';
import { AdminService } from '../../services/admin.service';

@Component({
  selector: 'app-usuario-password',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './usuario-password.component.html',
  styleUrl: './usuario-password.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuarioPasswordComponent {
  readonly usuario = input.required<UsuarioLista>();
  readonly enviado = output<void>();

  private readonly fb = inject(FormBuilder);
  private readonly admin = inject(AdminService);

  readonly guardando = signal(false);

  readonly formulario = this.fb.nonNullable.group(
    {
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
      confirmacion: ['', [Validators.required]],
    },
    { validators: this.coinciden }
  );

  restablecer(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    this.guardando.set(true);
    this.admin
      .restablecerPassword(this.usuario().id, this.formulario.getRawValue().password)
      .subscribe((ok) => {
        this.guardando.set(false);
        if (ok) {
          this.enviado.emit();
        }
      });
  }

  private coinciden(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value ?? '';
    const confirmacion = control.get('confirmacion')?.value ?? '';
    return password === confirmacion ? null : { noCoinciden: true };
  }
}