import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {MatTableDataSource} from '@angular/material/table';
import {MatDialog} from '@angular/material/dialog';
import {MatPaginator} from "@angular/material/paginator";
import {MatSort} from "@angular/material/sort";
import {Subject, catchError, of, merge} from 'rxjs';
import {debounceTime, distinctUntilChanged, filter, startWith, switchMap} from 'rxjs/operators';
import {AssetResponse, CreateAsset, UpdateAsset} from "../../core/models/asset.interface";
import {AssetService} from "../../core/services/asset.service";
import {QueryService} from "../../core/services/query.service";
import {BaseDialogComponent, DialogField} from 'src/app/shared/components/base-dialog/base-dialog.component';
import {Constants} from 'src/app/core/constants';
import {ConfirmDialogComponent} from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';
import {NotificationUtils} from '../../core/utils/notification.utils';
import {QueryParams} from '../../core/models/query-params.interface';
import {AssetDialogComponent} from './asset-dialog/asset-dialog.component';

@Component({
    selector: 'app-assets',
    templateUrl: './assets.component.html',
    styleUrls: ['./assets.component.scss']
})
export class AssetsComponent implements OnInit, AfterViewInit {
    public readonly displayedColumns: string[] = ['name', 'description', 'assetPath', 'createdAt', 'actions'];
    public readonly dataSource: MatTableDataSource<AssetResponse> = new MatTableDataSource();
    public searchTerm: Subject<string> = new Subject<string>();
    public isLoading: boolean = false;
    public totalItems: number = 0;
    public currentSearchValue: string = '';
    public errorMessage: string = '';

    @ViewChild(MatPaginator) private readonly paginator!: MatPaginator;
    @ViewChild(MatSort) private readonly sort!: MatSort;

    private readonly assetFields: DialogField[] = [
        {
            name: 'name',
            type: 'text',
            label: 'Name',
            required: true
        },
        {
            name: 'description',
            type: 'text',
            label: 'Description',
            required: false
        },
        {
            name: 'asset',
            type: 'file',
            label: 'File',
            required: true,
            accept: '*'
        }
    ];

    constructor(
        private readonly dialog: MatDialog,
        private readonly assetService: AssetService,
        private readonly queryService: QueryService
    ) {
    }

    ngOnInit(): void {
    }

    ngAfterViewInit(): void {
        this.loadAssets();
    }

    public loadAssets(): void {
        this.setupSearchAndSort();
    }

    public onView(asset: AssetResponse): void {
        this.dialog.open(AssetDialogComponent, {
            width: '800px',
            data: asset
        });
    }

    public onSearch(event: Event): void {
        const target = event.target as HTMLInputElement;
        this.currentSearchValue = target.value;
        this.searchTerm.next(this.currentSearchValue);
    }

    public onCreate(): void {
        const dialogRef = this.dialog.open(BaseDialogComponent<CreateAsset>, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                title: 'Create Asset',
                fields: this.assetFields,
                mode: 'create'
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter(result => !!result),
                switchMap(result => this.assetService.create(result as CreateAsset))
            )
            .subscribe({
                next: () => {
                    this.loadAssets();
                    NotificationUtils.showSuccess(this.dialog, 'Asset has been created successfully');
                },
                error: (error) => {
                    console.error('Error creating asset:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to create asset', error);
                }
            });
    }

    public onEdit(asset: AssetResponse): void {
        const dialogRef = this.dialog.open(BaseDialogComponent<UpdateAsset>, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                title: 'Edit Asset',
                fields: this.assetFields.filter(field => field.name !== 'asset'),
                mode: 'edit',
                entity: {
                    id: asset.id,
                    name: asset.name,
                    description: asset.description,
                    assetPath: asset.assetPath
                }
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter(result => !!result),
                switchMap(result => this.assetService.update({
                    id: asset.id,
                    ...result
                }))
            )
            .subscribe({
                next: () => {
                    this.loadAssets();
                    NotificationUtils.showSuccess(this.dialog, `Asset "${asset.name}" has been updated successfully`);
                },
                error: (error) => {
                    console.error('Error updating asset:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to update asset', error);
                }
            });
    }

    public onDelete(asset: AssetResponse): void {
        const dialogRef = this.dialog.open(ConfirmDialogComponent, {
            width: Constants.DIALOG_WIDTH,
            data: {
                title: 'Confirm Delete',
                message: `Are you sure you want to delete asset "${asset.name}"?`,
                confirmButton: 'Delete',
                cancelButton: 'Cancel'
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter((result: boolean) => result),
                switchMap(() => this.assetService.delete(asset.id)),
                catchError(error => {
                    NotificationUtils.showError(this.dialog, `Failed to delete asset ${asset.name}`, error);
                    return of(null);
                })
            )
            .subscribe(() => {
                this.loadAssets();
                NotificationUtils.showSuccess(this.dialog, `Asset "${asset.name}" has been deleted successfully`);
            });
    }

    private setupSearchAndSort(): void {
        merge(
            this.searchTerm.pipe(
                debounceTime(300),
                distinctUntilChanged(),
                filter(term => !term || term.length >= 3)
            ),
            this.sort.sortChange,
            this.paginator.page
        )
            .pipe(
                startWith({}),
                switchMap(() => {
                    this.isLoading = true;
                    const params: QueryParams = {
                        page: this.paginator.pageIndex + 1,
                        pageSize: this.paginator.pageSize,
                        searchTerm: this.currentSearchValue,
                        sortColumn: this.sort.active,
                        sortDirection: this.sort.direction as 'asc' | 'desc'
                    };

                    return this.currentSearchValue && this.currentSearchValue.length >= 3
                        ? this.assetService.findAssetsByTerm(this.currentSearchValue)
                        : this.assetService.getAssets(this.queryService.normaliseQueryParams(params));
                }),
                catchError(error => {
                    console.error('Error fetching assets:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to fetch assets', error);
                    return of({items: [], total: 0, page: 1, pageSize: 50});
                })
            )
            .subscribe((response: any) => {
                this.dataSource.data = response.items;
                this.totalItems = response.total;
                this.isLoading = false;
            });
    }
}
