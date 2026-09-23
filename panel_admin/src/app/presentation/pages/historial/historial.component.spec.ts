import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { HistorialPorteria } from '../../../domain/models/historial';
import { AdminRepository, ADMIN_REPOSITORY } from '../../../domain/repositories/admin.repository';
import { AdminService } from '../../services/admin.service';
import { HistorialComponent } from './historial.component';

const PORTERIA: HistorialPorteria[] = [
  {
    idRegistro: 2,
    nombreCompleto: 'Portero Parqueo',
    documentoIdentidad: '9999999',
    fechaEntrada: '2026-08-28T16:49:27',
    fechaSalida: '2026-08-28T17:09:02',
    puertaEntrada: 'puerta1',
    puertaSalida: null,
    motivoVisita: 'auditoria e2e',
    areaDestino: 'Oficinas',
    registradoPorNombre: 'Portero Portería',
    registradoPorDocumento: '7777777',
  },
  {
    idRegistro: 1,
    nombreCompleto: 'Visitante Antiguo',
    documentoIdentidad: '1111111',
    fechaEntrada: '2026-08-20T10:00:00',
    fechaSalida: '2026-08-20T11:00:00',
    puertaEntrada: null,
    puertaSalida: null,
    motivoVisita: null,
    areaDestino: null,
    registradoPorNombre: null,
    registradoPorDocumento: null,
  },
];

class FakeAdminRepo implements AdminRepository {
  listarUsuarios() {
    return of([]);
  }

  crearUsuario() {
    return of(1);
  }

  actualizarUsuario() {
    return of(void 0);
  }

  restablecerPassword() {
    return of(void 0);
  }

  obtenerHistorialPorteria() {
    return of(PORTERIA);
  }

  obtenerHistorialParqueo() {
    return of([]);
  }

  obtenerHistorialUsuario(_id: number) {
    return of({ nombreCompleto: '', documentoIdentidad: '', porteria: [], parqueo: [] });
  }

  obtenerAprobacionesOperador(_id: number) {
    return of({
      usuarioId: 0,
      nombreCompleto: '',
      documentoIdentidad: '',
      rol: '',
      desde: null,
      hasta: null,
      totalCompletadas: 0,
      totalPendientes: 0,
      porteria: [],
      parqueo: [],
    });
  }
}

describe('HistorialComponent — campo Aprobado por', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HistorialComponent],
      providers: [
        AdminService,
        { provide: ADMIN_REPOSITORY, useClass: FakeAdminRepo },
      ],
    }).compileComponents();
  });

  it('muestra el aprobador en el detalle y "—" para registros pre-auditoría', async () => {
    const fixture = TestBed.createComponent(HistorialComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const items = fixture.nativeElement.querySelectorAll('.historial__item');
    expect(items.length).toBe(2);

    // Primer registro: aprobado por Portero Portería
    (items[0] as HTMLElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const renglones = Array.from(
      fixture.nativeElement.querySelectorAll('.detalle__renglon')
    ).map((el) => {
      const renglon = el as HTMLElement;
      return {
        etiqueta: renglon.querySelector('dt')?.textContent?.trim(),
        valor: renglon.querySelector('dd')?.textContent?.trim(),
      };
    });
    const filaAprobado = renglones.find((r) => r.etiqueta === 'Aprobado por');
    expect(filaAprobado?.valor).toBe('Portero Portería');

    // Cerrar y abrir el registro pre-auditoría
    (fixture.nativeElement.querySelector('.detalle__cerrar') as HTMLElement).click();
    fixture.detectChanges();

    const items2 = fixture.nativeElement.querySelectorAll('.historial__item');
    (items2[1] as HTMLElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const renglones2 = Array.from(
      fixture.nativeElement.querySelectorAll('.detalle__renglon')
    ).map((el) => {
      const renglon = el as HTMLElement;
      return {
        etiqueta: renglon.querySelector('dt')?.textContent?.trim(),
        valor: renglon.querySelector('dd')?.textContent?.trim(),
      };
    });
    const filaAprobado2 = renglones2.find((r) => r.etiqueta === 'Aprobado por');
    expect(filaAprobado2?.valor).toBe('—');
  });
});