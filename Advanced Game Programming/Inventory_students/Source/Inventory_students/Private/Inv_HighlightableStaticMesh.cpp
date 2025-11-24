// Fill out your copyright notice in the Description page of Project Settings.


#include "Inv_HighlightableStaticMesh.h"

void UInv_HighlightableStaticMesh::Highlight_Implementation()
{
    SetOverlayMaterial(HighlightMaterial);
}

void UInv_HighlightableStaticMesh::UnHighlight_Implementation()
{
    SetOverlayMaterial(nullptr);
}
