import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { EMPTY, Observable, catchError, finalize, tap } from 'rxjs';
import { ListService, LocalizationPipe, LocalizationService, PagedResultDto } from '@abp/ng.core';
import {
  ButtonComponent,
  Confirmation,
  ConfirmationService,
  ModalCloseDirective,
  ModalComponent,
} from '@abp/ng.theme.shared';
import { GetPantryItemsInput, PantryItemDto, PantryItemService, UpdatePantryItemDto } from '../proxy/pantries';
import { toCalendarDate, validExpirationDate } from './pantry-date';

export type PantryViewState = 'loading' | 'error' | 'empty' | 'loaded';

export const MAX_UNIT_LENGTH = 20;
export const MAX_QUANTITY = 1_000_000;

/*
 * "Mi despensa" (RF-11). El servidor decide qué ítems son del usuario: la pantalla sólo muestra
 * lo que devuelve el proxy generado. Ocultar o deshabilitar botones no reemplaza la autorización.
 *
 * Errores: el listado y el guardado usan skipHandleError porque el componente los muestra en la
 * propia pantalla (estado de error con reintento / mensaje dentro del formulario). Marcar como
 * consumido y eliminar usan el manejo predeterminado de ABP.
 */
@Component({
  selector: 'app-pantry',
  templateUrl: './pantry.component.html',
  styleUrl: './pantry.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    NgbPaginationModule,
    ModalComponent,
    ModalCloseDirective,
    ButtonComponent,
    LocalizationPipe,
    DatePipe,
    DecimalPipe,
  ],
  providers: [ListService],
})
export class PantryComponent {
  readonly list = inject(ListService);
  private readonly pantryItemService = inject(PantryItemService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly localization = inject(LocalizationService);
  private readonly fb = inject(FormBuilder);

  readonly maxUnitLength = MAX_UNIT_LENGTH;
  readonly maxQuantity = MAX_QUANTITY;

  readonly state = signal<PantryViewState>('loading');
  readonly items = signal<PantryItemDto[]>([]);
  readonly totalCount = signal(0);
  readonly showConsumed = signal(true);

  readonly selectedItem = signal<PantryItemDto | undefined>(undefined);
  readonly isModalOpen = signal(false);
  readonly isSaving = signal(false);
  readonly saveError = signal<string | null>(null);

  readonly form = this.fb.group({
    quantity: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(MAX_QUANTITY)]),
    unit: this.fb.control<string>('', [Validators.maxLength(MAX_UNIT_LENGTH)]),
    expirationDate: this.fb.control<string>('', [validExpirationDate]),
  });

  readonly toCalendarDate = toCalendarDate;

  constructor() {
    this.list.maxResultCount = 10;
    this.list
      .hookToQuery(query => this.loadPage(query))
      .pipe(takeUntilDestroyed())
      .subscribe(result => {
        this.items.set(result.items ?? []);
        this.totalCount.set(result.totalCount ?? 0);
        this.state.set(result.items?.length ? 'loaded' : 'empty');
      });
  }

  retry(): void {
    this.list.get();
  }

  toggleShowConsumed(show: boolean): void {
    this.showConsumed.set(show);
    this.list.page = 0;
    this.list.get();
  }

  changePage(page: number): void {
    this.list.page = page - 1;
  }

  openEdit(item: PantryItemDto): void {
    this.selectedItem.set(item);
    this.saveError.set(null);
    this.form.reset({
      quantity: item.quantity ?? null,
      unit: item.unit ?? '',
      expirationDate: item.expirationDate ?? '',
    });
    this.isModalOpen.set(true);
  }

  save(): void {
    const item = this.selectedItem();
    if (!item || this.isSaving()) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const input: UpdatePantryItemDto = {
      quantity: value.quantity ?? null,
      unit: value.unit?.trim() || null,
      expirationDate: value.expirationDate || null,
    };

    this.isSaving.set(true);
    this.saveError.set(null);
    this.pantryItemService
      .update(item.id!, input, { skipHandleError: true })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.isModalOpen.set(false);
          this.list.get();
        },
        // Lo ingresado se conserva: sólo se muestra el motivo del rechazo.
        error: (error: HttpErrorResponse) => this.saveError.set(this.getServerMessage(error)),
      });
  }

  markAsConsumed(item: PantryItemDto): void {
    this.pantryItemService.markAsConsumed(item.id!).subscribe(() => this.list.get());
  }

  delete(item: PantryItemDto): void {
    this.confirmation
      .warn('::Pantry:DeleteConfirmation', '::Pantry:AreYouSure', {
        messageLocalizationParams: [item.productName ?? ''],
      })
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.pantryItemService.delete(item.id!).subscribe(() => this.list.get());
        }
      });
  }

  private loadPage(query: { skipCount?: number; maxResultCount?: number; sorting?: string }): Observable<PagedResultDto<PantryItemDto>> {
    const input: GetPantryItemsInput = {
      skipCount: query.skipCount,
      maxResultCount: query.maxResultCount ?? this.list.maxResultCount,
      sorting: query.sorting || undefined,
      includeConsumed: this.showConsumed(),
    };

    this.state.set('loading');
    return this.pantryItemService.getList(input, { skipHandleError: true }).pipe(
      tap({ subscribe: () => this.state.set('loading') }),
      catchError(() => {
        this.state.set('error');
        return EMPTY;
      }),
    );
  }

  private getServerMessage(error: HttpErrorResponse): string {
    const serverError = error?.error?.error;
    const validationMessages: string[] = (serverError?.validationErrors ?? [])
      .map((validationError: { message?: string }) => validationError.message)
      .filter((message: string | undefined): message is string => !!message);

    if (validationMessages.length) {
      return validationMessages.join(' ');
    }

    return serverError?.message || this.localization.instant('::Pantry:SaveError');
  }
}
