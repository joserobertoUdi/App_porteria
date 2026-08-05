import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AuthSessionService } from '../../services/auth-session.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly session = inject(AuthSessionService);
  private readonly router = inject(Router);
  private readonly destruir = new Subject<void>();

  readonly formulario = this.fb.nonNullable.group({
    documento: ['', Validators.required],
    password: ['', Validators.required],
  });

  readonly cargando = this.session.cargando;
  readonly error = this.session.errorLogin;

  ngOnInit(): void {
    if (this.session.isAuthenticated() && this.session.esAdmin()) {
      void this.router.navigate(['/']);
    }
  }

  iniciarSesion(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const { documento, password } = this.formulario.getRawValue();
    this.session
      .login({ documentoIdentidad: documento, password })
      .pipe(takeUntil(this.destruir))
      .subscribe((ok) => {
        if (ok) {
          void this.router.navigate(['/']);
        }
      });
  }

  ngOnDestroy(): void {
    this.destruir.next();
    this.destruir.complete();
  }
}