import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { AssetResponse, CreateAsset, UpdateAsset, AssetResponseDto } from "../../core/models/asset.interface";
import { AssetService } from "../../core/services/asset.service";
import { merge, of } from "rxjs";
import { startWith, switchMap, filter, catchError } from "rxjs/operators";
import { MatPaginator } from "@angular/material/paginator";
import { MatSort } from "@angular/material/sort";
import { QueryService } from "../../core/services/query.service";
import { BaseDialogComponent } from 'src/app/shared/components/base-dialog/base-dialog.component';
import { Constants } from 'src/app/core/constants';
import { ConfirmDialogComponent } from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-assets',
    templateUrl: './assets.component.html',
    styleUrls: ['./assets.component.scss']
})
export class AssetsComponent implements OnInit, AfterViewInit {
    displayedColumns: string[] = ['name', 'description', 'assetPath', 'createdAt', 'actions'];
    dataSource = new MatTableDataSource<AssetResponse>();
    totalItems = 0;
    isLoading = true;
    currentSearchValue = '';

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    constructor(
        private readonly assetService: AssetService,
        private readonly queryService: QueryService,
        public dialog: MatDialog
    ) { }

    ngOnInit(): void {
        this.loadAssets();
    }

    ngAfterViewInit(): void {
        merge(this.sort.sortChange, this.paginator.page)
            .pipe(
                startWith({}),
                switchMap(() => {
                    this.isLoading = true;
                    return this.assetService.getAssets({
                        page: this.paginator.pageIndex + 1,
                        pageSize: this.paginator.pageSize,
                        sortColumn: this.sort.active,
                        sortDirection: this.sort.direction as 'asc' | 'desc',
                        searchTerm: this.currentSearchValue
                    }).pipe(catchError(() => of(null)));
                })
            )
            .subscribe({
                next: (response: AssetResponseDto | null) => {
                    if (response) {
                        this.dataSource.data = response.items;
                        this.totalItems = response.total;
                    }
                    this.isLoading = false;
                }
            });
    }

    loadAssets(): void {
        this.assetService.getAssets({
            page: this.paginator.pageIndex + 1,
            pageSize: this.paginator.pageSize,
            searchTerm: this.currentSearchValue
        }).subscribe({
            next: (response: AssetResponseDto) => {
                this.dataSource.data = response.items;
                this.totalItems = response.total;
                this.isLoading = false;
            }
        });
    }

    onSearch(event: Event): void {
        const target = event.target as HTMLInputElement;
        this.currentSearchValue = target.value;
        this.loadAssets();
    }

    onCreate(): void {
        const dialogRef = this.dialog.open(BaseDialogComponent<CreateAsset>, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                title: 'Create Asset',
                fields: [
                    {
                        name: 'name',
                        type: 'text',
                        label: 'Name',
                        required: true
                    },
                    {
                        name: 'description',
                        type: 'text',
                        label: 'Description'
                    },
                    {
                        name: 'assetPath',
                        type: 'text',
                        label: 'Asset Path',
                        required: true
                    },
                    {
                        name: 'asset',
                        type: 'file',
                        label: 'File',
                        required: true,
                        accept: '*',
                    }
                ]
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter((result: CreateAsset) => !!result),
                switchMap((result: CreateAsset) => this.assetService.create(result))
            )
            .subscribe({
                next: () => {
                    this.loadAssets();
                },
                error: (error) => {
                    console.error('Error creating asset:', error);
                }
            });
    }

    onEdit(asset: AssetResponse): void {
        const dialogRef = this.dialog.open(BaseDialogComponent<UpdateAsset>, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                title: 'Edit Asset',
                fields: [
                    {
                        name: 'name',
                        type: 'text',
                        label: 'Name',
                        required: true,
                        value: asset.name
                    },
                    {
                        name: 'description',
                        type: 'text',
                        label: 'Description',
                        value: asset.description
                    },
                    {
                        name: 'assetPath',
                        type: 'text',
                        label: 'Asset Path',
                        required: true,
                        value: asset.assetPath
                    }
                ]
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter((result: UpdateAsset) => !!result),
                switchMap((result: UpdateAsset) => this.assetService.update({
                    ...result
                }))
            )
            .subscribe({
                next: () => {
                    this.loadAssets();
                },
                error: (error) => {
                    console.error('Error updating asset:', error);
                }
            });
    }

    onDelete(asset: AssetResponse): void {
        const dialogRef = this.dialog.open(ConfirmDialogComponent, {
            width: Constants.DIALOG_WIDTH,
            data: {
                title: 'Confirm Delete',
                message: `Are you sure you want to delete asset "${asset.name}"?`,
                confirmButton: 'Delete',
                cancelButton: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe((confirmed: boolean) => {
            if (confirmed) {
                this.assetService.delete(asset.id)
                    .subscribe({
                        next: () => {
                            this.loadAssets();
                        },
                        error: (error) => {
                            console.error('Error deleting asset:', error);
                        }
                    });
            }
        });
    }
}
