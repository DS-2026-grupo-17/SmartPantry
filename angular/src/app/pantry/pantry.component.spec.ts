import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, Subject, of, startWith, switchMap, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ListService, LocalizationService } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { PantryItemDto, PantryItemService } from '../proxy/pantries';
import { PantryComponent } from './pantry.component';

// ListService simplificado: re-ejecuta la consulta cada vez que se llama a get().
class FakeListService {
  page = 0;
  maxResultCount = 10;
  private readonly refresh$ = new Subject<void>();

  hookToQuery<T>(callback: (query: { skipCount: number; maxResultCount: number; sorting: string }) => Observable<T>) {
    return this.refresh$.pipe(
      startWith(undefined),
      switchMap(() => callback({ skipCount: this.page * this.maxResultCount, maxResultCount: this.maxResultCount, sorting: '' })),
    );
  }

  get() {
    this.refresh$.next();
  }
}

const ITEM: PantryItemDto = {
  id: 'item-1',
  productId: 'product-1',
  productName: 'Nutella',
  productBarcode: '3017620422003',
  quantity: 1,
  unit: 'frasco',
  expirationDate: '2026-10-26',
  isConsumed: false,
};

describe('PantryComponent', () => {
  let pantryItemService: {
    getList: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
    delete: ReturnType<typeof vi.fn>;
    markAsConsumed: ReturnType<typeof vi.fn>;
  };
  let confirmation: { warn: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    pantryItemService = {
      getList: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
      markAsConsumed: vi.fn(),
    };
    confirmation = { warn: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: PantryItemService, useValue: pantryItemService },
        { provide: ConfirmationService, useValue: confirmation },
        { provide: LocalizationService, useValue: { instant: (key: string) => key } },
        { provide: ListService, useClass: FakeListService },
      ],
    });
  });

  function createComponent(): PantryComponent {
    return TestBed.runInInjectionContext(() => new PantryComponent());
  }

  describe('listado', () => {
    it('muestra el estado de carga mientras la respuesta está pendiente (sin lista vacía)', () => {
      pantryItemService.getList.mockReturnValue(new Subject());

      const component = createComponent();

      expect(component.state()).toBe('loading');
      expect(component.items()).toEqual([]);
    });

    it('muestra el estado de error y permite reintentar', () => {
      pantryItemService.getList.mockReturnValueOnce(throwError(() => new HttpErrorResponse({ status: 0 })));

      const component = createComponent();
      expect(component.state()).toBe('error');

      pantryItemService.getList.mockReturnValueOnce(of({ items: [ITEM], totalCount: 1 }));
      component.retry();

      expect(component.state()).toBe('loaded');
      expect(pantryItemService.getList).toHaveBeenCalledTimes(2);
    });

    it('muestra el estado vacío cuando el usuario no tiene ítems', () => {
      pantryItemService.getList.mockReturnValue(of({ items: [], totalCount: 0 }));

      const component = createComponent();

      expect(component.state()).toBe('empty');
    });

    it('muestra los resultados y maneja los errores del listado en la pantalla', () => {
      pantryItemService.getList.mockReturnValue(of({ items: [ITEM], totalCount: 1 }));

      const component = createComponent();

      expect(component.state()).toBe('loaded');
      expect(component.items()).toEqual([ITEM]);
      expect(component.totalCount()).toBe(1);
      expect(pantryItemService.getList).toHaveBeenCalledWith(
        expect.objectContaining({ skipCount: 0, maxResultCount: 10, includeConsumed: true }),
        { skipHandleError: true },
      );
    });

    it('vuelve a consultar sin los consumidos al cambiar el filtro', () => {
      pantryItemService.getList.mockReturnValue(of({ items: [ITEM], totalCount: 1 }));
      const component = createComponent();

      component.toggleShowConsumed(false);

      expect(pantryItemService.getList).toHaveBeenLastCalledWith(
        expect.objectContaining({ includeConsumed: false }),
        { skipHandleError: true },
      );
    });
  });

  describe('edición', () => {
    let component: PantryComponent;

    beforeEach(() => {
      pantryItemService.getList.mockReturnValue(of({ items: [ITEM], totalCount: 1 }));
      component = createComponent();
      component.openEdit(ITEM);
    });

    it('precarga cantidad, unidad y fecha', () => {
      expect(component.form.getRawValue()).toEqual({ quantity: 1, unit: 'frasco', expirationDate: '2026-10-26' });
      expect(component.isModalOpen()).toBe(true);
    });

    it('no invoca al servidor con cantidad negativa', () => {
      component.form.controls.quantity.setValue(-1);

      component.save();

      expect(component.form.controls.quantity.hasError('min')).toBe(true);
      expect(pantryItemService.update).not.toHaveBeenCalled();
    });

    it('no invoca al servidor con una fecha inválida', () => {
      component.form.controls.expirationDate.setValue('1900-01-01');

      component.save();

      expect(component.form.controls.expirationDate.hasError('invalidDate')).toBe(true);
      expect(pantryItemService.update).not.toHaveBeenCalled();
    });

    it('guarda datos válidos con el proxy y actualiza la lista', () => {
      pantryItemService.update.mockReturnValue(of({ ...ITEM, quantity: 2 }));
      component.form.setValue({ quantity: 2, unit: ' frasco ', expirationDate: '' });

      component.save();

      expect(pantryItemService.update).toHaveBeenCalledWith(
        'item-1',
        { quantity: 2, unit: 'frasco', expirationDate: null },
        { skipHandleError: true },
      );
      expect(component.isModalOpen()).toBe(false);
      expect(component.isSaving()).toBe(false);
      expect(pantryItemService.getList).toHaveBeenCalledTimes(2);
    });

    it('muestra el mensaje del servidor sin perder lo ingresado', () => {
      pantryItemService.update.mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 400, error: { error: { message: 'Datos inválidos' } } })),
      );
      component.form.setValue({ quantity: 5, unit: 'kg', expirationDate: '2026-11-01' });

      component.save();

      expect(component.saveError()).toBe('Datos inválidos');
      expect(component.isModalOpen()).toBe(true);
      expect(component.form.getRawValue()).toEqual({ quantity: 5, unit: 'kg', expirationDate: '2026-11-01' });
    });
  });

  describe('consumo y eliminación', () => {
    let component: PantryComponent;

    beforeEach(() => {
      pantryItemService.getList.mockReturnValue(of({ items: [ITEM], totalCount: 1 }));
      component = createComponent();
    });

    it('marca un ítem como consumido y actualiza la lista', () => {
      pantryItemService.markAsConsumed.mockReturnValue(of({ ...ITEM, isConsumed: true }));

      component.markAsConsumed(ITEM);

      expect(pantryItemService.markAsConsumed).toHaveBeenCalledWith('item-1');
      expect(pantryItemService.getList).toHaveBeenCalledTimes(2);
    });

    it('pide confirmación y elimina al confirmar', () => {
      confirmation.warn.mockReturnValue(of(Confirmation.Status.confirm));
      pantryItemService.delete.mockReturnValue(of(undefined));

      component.delete(ITEM);

      expect(confirmation.warn).toHaveBeenCalledWith('::Pantry:DeleteConfirmation', '::Pantry:AreYouSure', {
        messageLocalizationParams: ['Nutella'],
      });
      expect(pantryItemService.delete).toHaveBeenCalledWith('item-1');
      expect(pantryItemService.getList).toHaveBeenCalledTimes(2);
    });

    it('no llama al servidor si se cancela la confirmación', () => {
      confirmation.warn.mockReturnValue(of(Confirmation.Status.reject));

      component.delete(ITEM);

      expect(confirmation.warn).toHaveBeenCalled();
      expect(pantryItemService.delete).not.toHaveBeenCalled();
    });
  });
});
