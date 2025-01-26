export interface AssetResponse {
    id: number;
    name: string;
    description: string | null;
    assetPath: string;
    createdAt: Date;
    createdBy: number;
    updatedAt: Date | null;
    updatedBy: number | null;
}

export interface CreateAsset {
    name: string;
    description?: string;
    assetPath: string;
}

export interface UpdateAsset extends CreateAsset {
    id: number;
}

export interface AssetResponseDto {
    items: AssetResponse[];
    total: number;
    page: number;
    pageSize: number;
}
