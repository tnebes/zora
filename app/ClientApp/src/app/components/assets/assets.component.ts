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
import {AssetUnifiedComponent} from './asset-unified/asset-unified.component';

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
    public downloadingAssets: Set<number> = new Set<number>();

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
        this.dialog.open(AssetUnifiedComponent, {
            width: '800px',
            data: {
                mode: 'view',
                asset: asset
            }
        });
    }

    public onSearch(event: Event): void {
        const target = event.target as HTMLInputElement;
        this.currentSearchValue = target.value;
        this.searchTerm.next(this.currentSearchValue);
    }

    public onCreate(): void {
        const dialogRef = this.dialog.open(AssetUnifiedComponent, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                mode: 'upload'
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter(result => !!result)
            )
            .subscribe({
                next: () => {
                    this.loadAssets();
                    NotificationUtils.showSuccess(this.dialog, 'Asset has been created successfully');
                }
            });
    }

    public onEdit(asset: AssetResponse): void {
        const dialogRef = this.dialog.open(AssetUnifiedComponent, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                mode: 'edit',
                asset: asset
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter(result => !!result)
            )
            .subscribe({
                next: () => {
                    this.loadAssets();
                    NotificationUtils.showSuccess(this.dialog, `Asset "${asset.name}" has been updated successfully`);
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

    public onDownload(asset: AssetResponse): void {
        if (this.downloadingAssets.has(asset.id)) {
            return;
        }

        this.downloadingAssets.add(asset.id);
        this.assetService.download(asset.id).subscribe({
            next: (blob: Blob) => {
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                const originalExtension = asset.assetPath.split('.').pop() || '';
                const fileName = asset.name.includes('.') ? asset.name : `${asset.name}.${originalExtension}`;
                link.download = fileName;
                link.click();
                window.URL.revokeObjectURL(url);
                this.downloadingAssets.delete(asset.id);
            },
            error: (error) => {
                this.downloadingAssets.delete(asset.id);
                console.error('Error downloading asset:', error);
                NotificationUtils.showError(this.dialog, `Failed to download asset "${asset.name}"`, error);
            }
        });
    }

    public isDownloading(assetId: number): boolean {
        return this.downloadingAssets.has(assetId);
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
