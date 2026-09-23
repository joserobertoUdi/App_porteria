import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import {
  AprobacionesOperador,
} from '../../../domain/models/aprobaciones';
import { AdminRepository, ADMIN_REPOSITORY } from '../../../domain/repositories/admin.repository';
import { UsuarioLista } from '../../../domain/models/usuario';
import { AdminService } from '../../services/admin.service';
import { AuditoriaComponent } from './auditoria.component';

const USUARIOS: UsuarioLista[] = [
  {
    id: 1,
    nombreCompleto: 'Administrador',
    documentoIdentidad: 'admin',
    tipoUsuario: 'Trabajador',
    rol: 'Administrador',
    estado: true,
    fechaRegistro: '2026-08-01T10:00:00',
  },
  {
    id: 3,
    nombreCompleto: 'Portero Portería',
    documentoIdentidad: '7777777',
    tipoUsuario: 'Trabajador',
    rol: 'PorteroPorteria',
    estado: true,
    fechaRegistro: '2026-08-01T10:00:00',
  },
  {
    id: 2,
    nombreCompleto: 'Portero Parqueo',
    documentoIdentidad: '8888888',
    tipoUsuario: 'Trabajador',
    rol: 'PorteroParqueo',
    estado: true,
    fechaRegistro: '2026-08-01T10:00:00',
  },
];

const PORTERIA_RESUMEN: AprobacionesOperador = {
  usuarioId: 3,
  nombreCompleto: 'Portero Portería',
  documentoIdentidad: '7777777',
  rol: 'PorteroPorteria',
  desde: null,
  hasta: null,
  totalCompletadas: 1,
  totalPendientes: 1,
  porteria: [
    {
      idRegistro: 2,
      fechaEntrada: '2026-08-28T16:49:27',
      fechaSalida: '2026-08-28T17:09:02',
      puertaEntrada: 'puerta1',
      puertaSalida: 'puerta2',
      motivoVisita: 'auditoria e2e',
      areaDestino: 'Oficinas',
      visitanteNombre: 'Portero Parqueo',
      visitanteDocumento: '9999999',
      estaCompletada: true,
    },
    {
      idRegistro: 3,
      fechaEntrada: '2026-08-28T17:30:00',
      fechaSalida: null,
      puertaEntrada: 'puerta1',
      puertaSalida: null,
      motivoVisita: 'reunión',
      areaDestino: null,
      visitanteNombre: 'Visitante A',
      visitanteDocumento: '1111111',
      estaCompletada: false,
    },
  ],
  parqueo: [
    {
      idRegistro: 1,
      fechaIngreso: '2026-08-28T17:20:00',
      fechaSalida: null,
      puertaAcceso: 'rampa',
      matricula: '1234ABC',
      marca: 'Toyota',
      modelo: null,
      visitanteNombre: 'Visitante Parqueo',
      visitanteDocumento: '5555555',
      estaCompletada: false,
    },
  ],
};

const PARQUEO_RESUMEN: AprobacionesOperador = {
  usuarioId: 2,
  nombreCompleto: 'Portero Parqueo',
  documentoIdentidad: '8888888',
  rol: 'PorteroParqueo',
  desde: null,
  hasta: null,
  totalCompletadas: 0,
  totalPendientes: 0,
  porteria: [],
  parqueo: [],
};

class FakeAdminRepo implements AdminRepository {
  llamadas: string[] = [];

  listarUsuarios() {
    return of(USUARIOS);
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
    return of([]);
  }

  obtenerHistorialParqueo() {
    return of([]);
  }

  obtenerHistorialUsuario(_id: number) {
    return of({ nombreCompleto: '', documentoIdentidad: '', porteria: [], parqueo: [] });
  }

  obtenerAprobacionesOperador(id: number, fecha?: string) {
    this.llamadas.push(`${id}|${fecha ?? ''}`);
    return of(id === 3 ? PORTERIA_RESUMEN : PARQUEO_RESUMEN);
  }
}

describe('AuditoriaComponent (vista de auditoría de guardias)', () => {
  async function setup() {
    const repo = new FakeAdminRepo();
    await TestBed.configureTestingModule({
      imports: [AuditoriaComponent],
      providers: [{ provide: ADMIN_REPOSITORY, useValue: repo }],
    }).compileComponents();

    const fixture = TestBed.createComponent(AuditoriaComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, repo };
  }

  it('solo lista usuarios con rol de guardia', async () => {
    const { fixture } = await setup();

    const items = fixture.nativeElement.querySelectorAll('.auditoria__item');
    expect(items.length).toBe(2);
    expect(fixture.nativeElement.textContent).not.toContain('Administrador');
    expect(items[0].textContent).toContain('Portero Portería');
    expect(items[1].textContent).toContain('Portero Parqueo');
  });

  it('abre el detalle del guardia con resumen, secciones y estados', async () => {
    const { fixture } = await setup();

    const items = fixture.nativeElement.querySelectorAll('.auditoria__item');
    (items[0] as HTMLElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('Portero Portería');
    expect(texto).toContain('Todos los días');

    // Chips de resumen
    expect(texto).toContain('Completadas');
    expect(texto).toContain('Pendientes');

    // Secciones
    expect(texto).toContain('PORTERÍA');
    expect(texto).toContain('PARQUEO');

    // Items con estado
    const estados = Array.from(
      fixture.nativeElement.querySelectorAll('.ag__estado')
    ).map((el) => (el as HTMLElement).textContent?.trim());
    expect(estados).toContain('Completada');
    expect(estados).toContain('Pendiente');

    expect(texto).toContain('Portero Parqueo');
    expect(texto).toContain('1234ABC · Visitante Parqueo');
    expect(texto).toContain('Motivo: auditoria e2e');
  });

  it('filtra por día y vuelve a consultar con la fecha', async () => {
    const { fixture, repo } = await setup();

    const items = fixture.nativeElement.querySelectorAll('.auditoria__item');
    (items[0] as HTMLElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(repo.llamadas.at(-1)).toBe('3|');

    const input = fixture.nativeElement.querySelector('input[type="date"]') as HTMLInputElement;
    input.value = '2026-08-28';
    input.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(repo.llamadas.at(-1)).toBe('3|2026-08-28');
    expect(fixture.nativeElement.textContent).toContain('Día: 2026-08-28');
  });

  it('muestra un guardia sin aprobaciones como vacío', async () => {
    const { fixture } = await setup();

    const items = fixture.nativeElement.querySelectorAll('.auditoria__item');
    (items[1] as HTMLElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Sin aprobaciones en el período');
    expect(fixture.nativeElement.textContent).toContain('Portero Parqueo');
  });
});

describe('AuditoriaComponent con AdminService real', () => {
  it('se resuelve dentro del árbol de servicios', async () => {
    const repo = new FakeAdminRepo();
    await TestBed.configureTestingModule({
      imports: [AuditoriaComponent],
      providers: [
        AdminService,
        { provide: ADMIN_REPOSITORY, useValue: repo },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(AuditoriaComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.auditoria__item').length).toBe(2);
  });
});