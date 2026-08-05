import { ChangeDetectionStrategy, Component, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CATALOGO_ROLES, CATALOGO_TIPOS_USUARIO } from '../../../core/constants/app.constants';
import { UsuarioLista } from '../../../domain/models/usuario';
import { AdminService } from '../../services/admin.service';

export type ModoUsuario = 'crear' | 'editar';

@Component({
  selector: 'app-usuario-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './usuario-form.component.html',
  styleUrl: './usuario-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuarioFormComponent {
  readonly modo = input.required<ModoUsuario>();
  readonly usuario = input<UsuarioLista | null>(null);
  readonly enviado = output<void>();

  private readonly fb = inject(FormBuilder);
  private readonly admin = inject(AdminService);

  readonly roles = CATALOGO_ROLES;
  readonly tipos = CATALOGO_TIPOS_USUARIO;
  readonly guardando = signal(false);

  readonly formulario = this.fb.nonNullable.group({
    nombreCompleto: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    documentoIdentidad: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]],
    tipoUsuarioId: [1, Validators.required],
    rolId: [3, Validators.required],
    password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
    estado: [true],
  });

  constructor() {
    effect(() => {
      const crear = this.modo() === 'crear';
      const documento = this.formulario.controls.documentoIdentidad;
      const password = this.formulario.controls.password;

      if (crear) {
        documento.enable();
        documento.setValidators([Validators.required, Validators.minLength(3), Validators.maxLength(20)]);
        password.setValidators([Validators.required, Validators.minLength(6), Validators.maxLength(100)]);
      } else {
        documento.disable();
        documento.clearValidators();
        password.clearValidators();
        password.setValue('');
      }
      documento.updateValueAndValidity();
      password.updateValueAndValidity();
    });

    effect(() => {
      const usuario = this.usuario();
      if (usuario) {
        this.formulario.patchValue({
          nombreCompleto: usuario.nombreCompleto,
          tipoUsuarioId: this.idPorNombre(this.tipos, usuario.tipoUsuario),
          rolId: this.idPorNombre(this.roles, usuario.rol),
          estado: usuario.estado,
        });
      }
    });
  }

  guardar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    this.guardando.set(true);
    const valores = this.formulario.getRawValue();
    const esCrear = this.modo() === 'crear';

    const operacion = esCrear
      ? this.admin.crearUsuario({
          nombreCompleto: valores.nombreCompleto.trim(),
          documentoIdentidad: valores.documentoIdentidad.trim(),
          tipoUsuarioId: valores.tipoUsuarioId,
          password: valores.password,
          rolId: valores.rolId,
        })
      : this.admin.actualizarUsuario(this.usuario()!.id, {
          nombreCompleto: valores.nombreCompleto.trim(),
          tipoUsuarioId: valores.tipoUsuarioId,
          rolId: valores.rolId,
          estado: valores.estado,
        });

    operacion.subscribe((ok) => {
      this.guardando.set(false);
      if (ok) {
        this.enviado.emit();
      }
    });
  }

  private idPorNombre(coleccion: readonly { id: number; nombre: string }[], nombre: string): number {
    const coincidencia = coleccion.find(
      (item) => item.nombre.toLocaleLowerCase() === (nombre ?? '').toLocaleLowerCase()
    );
    return coincidencia?.id ?? 1;
  }
}