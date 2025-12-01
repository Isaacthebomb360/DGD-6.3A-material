// Fill out your copyright notice in the Description page of Project Settings.


#include "Inv_InventoryGrid.h"
#include "Inv_GridSlot.h"
#include "Components/CanvasPanel.h"
#include "Components/CanvasPanelSlot.h"
#include "Inv_WidgetUtils.h"
#include "Blueprint/WidgetLayoutLibrary.h"

void UInv_InventoryGrid::NativeOnInitialized()
{
    Super::NativeOnInitialized();

    ConstructGrid();
}

void UInv_InventoryGrid::ConstructGrid()
{
    GridSlots.Reserve(Rows * Columns);

    for (int j = 0; j < Rows; j++)
    {
        for (int i = 0; i < Columns; i++)
        {


            UInv_GridSlot* GridSlot = CreateWidget<UInv_GridSlot>(this, GridSlotClass);
            CanvasPanel->AddChild(GridSlot);

            //model logic GridSlow Widget

            const FIntPoint TilePosition(i, j);

            GridSlot->SetTileIndex(UInv_WidgetUtils::GetIndexFromPosition(TilePosition, Columns));

            //visual logic GridSlot Widget 

            UCanvasPanelSlot* GridCPS = UWidgetLayoutLibrary::SlotAsCanvasSlot(GridSlot);

            GridCPS->SetSize(FVector2D(TileSize));

            GridCPS->SetPosition(TilePosition * TileSize);
        }
    }
}
