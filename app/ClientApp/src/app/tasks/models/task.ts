export interface Task {
    id: number;
    title: string;
    description: string;
    dueDate?: Date;
    status: string;
    assignedUserId?: number;
    assignedUserName?: string;
    createdAt: Date;
    updatedAt: Date;
    programId?: number;
    programName?: string;
    projectId?: number;
    projectName?: string;
}

export interface TaskResponseDto {
    items: Task[];
    totalCount: number;
    pageSize: number;
    currentPage: number;
}
