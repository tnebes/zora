import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {MatTableDataSource} from '@angular/material/table';
import {Router} from '@angular/router';

interface EntityColumn {
    id: string;
    label: string;
}

@Component({
    selector: 'app-entity-display-dialog',
    templateUrl: './entity-display-dialog.component.html',
    styleUrls: ['./entity-display-dialog.component.scss']
})
export class EntitySelectorDialogComponent implements OnInit {
    dataSource: MatTableDataSource<any> = new MatTableDataSource();
    displayedColumns: EntityColumn[] = [];
    columnIds: string[] = [];

    constructor(
        @Inject(MAT_DIALOG_DATA) public data: { entities: any[]; columns: EntityColumn[] },
        private dialogRef: MatDialogRef<EntitySelectorDialogComponent>,
        private router: Router
    ) {
    }

    ngOnInit(): void {
        this.dataSource.data = this.data.entities;
        this.displayedColumns = this.data.columns;
        this.columnIds = this.data.columns.map(c => c.id).concat('actions');
    }

    navigateToEntity(entity: any): void {
        const id = entity.id;
        if (id) {
            this.router.navigate([`/${this.data.entities[0].type}/${id}`]);
            this.dialogRef.close();
        }
    }
}
