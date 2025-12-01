// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "Inv_GridTypes.h"
#include "Inv_InventoryGrid.generated.h"

class UInv_GridSlot;
class UCanvasPanel;
/**
 * 
 */
UCLASS()
class INVENTORY_STUDENTS_API UInv_InventoryGrid : public UUserWidget
{
	GENERATED_BODY()
public:
	EInv_ItemCategory GetItemCategory() const { return ItemCategory; }

	virtual void NativeOnInitialized() override;

private:
	UPROPERTY(EditAnywhere, BlueprintReadOnly, meta = (AllowPrivateAccess = "true"), 
		Category="Inventory")
	EInv_ItemCategory ItemCategory;

	void ConstructGrid();

	UPROPERTY()
	TArray<TObjectPtr<UInv_GridSlot>> GridSlots;

	UPROPERTY(EditAnywhere, Category = "Inventory")
	TSubclassOf<UInv_GridSlot> GridSlotClass;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UCanvasPanel> CanvasPanel;

	UPROPERTY(EditAnywhere, Category = "Inventory")
	int32 Rows;

	UPROPERTY(EditAnywhere, Category = "Inventory")
	int32 Columns;

	UPROPERTY(EditAnywhere, Category = "Inventory")
	float TileSize;
};
