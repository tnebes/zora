import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { switchMap, filter, map } from 'rxjs/operators';
import { UserResponse } from '../core/models/user.interface';
import { UserService } from '../core/services/user.service';
import { AuthenticationService } from '../core/services/authentication.service';
import { BaseDialogComponent, DialogField } from '../shared/components/base-dialog/base-dialog.component';
import { Constants } from '../core/constants';
import { NotificationUtils } from '../core/utils/notification.utils';
import { Validators } from '@angular/forms';

@Component({
    selector: 'app-profile',
    templateUrl: './profile.component.html',
    styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
    public currentUser$: Observable<UserResponse>;
    
    private readonly userFields: DialogField[] = [
        {
            name: 'username',
            type: 'text',
            label: 'Username',
            required: true,
            validators: [Validators.minLength(3)]
        },
        {
            name: 'email',
            type: 'email',
            label: 'Email',
            required: true,
            validators: [Validators.email]
        }
    ];

    constructor(
        private readonly userService: UserService,
        private readonly authService: AuthenticationService,
        private readonly dialog: MatDialog
    ) {
        this.currentUser$ = this.authService.currentUser$.pipe(
            filter(user => !!user?.id),
            switchMap(user => this.userService.getUserById(Number(user!.id)))
        );
    }

    ngOnInit(): void {
    }

    public formatRoles(roles: { [key: number]: string }): string {
        return Object.values(roles).join(', ') || 'No roles assigned';
    }

    public onEdit(user: UserResponse): void {
        const dialogRef = this.dialog.open(BaseDialogComponent, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                title: 'Edit Profile',
                fields: this.userFields,
                mode: 'edit',
                entity: {
                    username: user.username,
                    email: user.email
                }
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter(result => !!result),
                switchMap(result => this.userService.updateUser({
                    id: user.id,
                    username: result.username,
                    email: result.email,
                    roleIds: Object.keys(user.roles).map(Number)
                }))
            )
            .subscribe({
                next: () => {
                    NotificationUtils.showSuccess(this.dialog, 'Profile updated successfully');
                },
                error: (error) => {
                    console.error('Error updating profile:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to update profile', error);
                }
            });
    }
} 