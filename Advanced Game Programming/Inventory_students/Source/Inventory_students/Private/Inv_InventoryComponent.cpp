// Fill out your copyright notice in the Description page of Project Settings.


#include "Inv_InventoryComponent.h"
#include "Inv_InventoryBase.h"

// Sets default values for this component's properties
UInv_InventoryComponent::UInv_InventoryComponent()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = false;

	// ...
}

void UInv_InventoryComponent::ToggleInventoryMenu()
{
	if (bInventoryMenuOpen)
	{
		CloseInventoryMenu();
	}
	else
	{
		OpenInventoryMenu();
	}
}


// Called when the game starts
void UInv_InventoryComponent::BeginPlay()
{
	Super::BeginPlay();

	ConstructInventory();
	
}

void UInv_InventoryComponent::ConstructInventory()
{
	OwningController = Cast<APlayerController>(GetOwner());

	checkf(OwningController.IsValid(), 
		TEXT("InventoryComponent should have a Player Controller as a Owner"));

	InventoryMenu = CreateWidget<UInv_InventoryBase>(OwningController.Get(),
		InventoryMenuClass);

	InventoryMenu->AddToViewport();

	CloseInventoryMenu();
}

void UInv_InventoryComponent::OpenInventoryMenu()
{
	//safe check of the InventoryMenu pointer
	if (!IsValid(InventoryMenu)) return;

	InventoryMenu->SetVisibility(ESlateVisibility::Visible);
	bInventoryMenuOpen = true;

	if (!OwningController.IsValid()) return;

	FInputModeGameAndUI InputMode;
	OwningController->SetInputMode(InputMode);
	OwningController->SetShowMouseCursor(true);
}

void UInv_InventoryComponent::CloseInventoryMenu()
{
	if (!IsValid(InventoryMenu)) return;

	InventoryMenu->SetVisibility(ESlateVisibility::Collapsed);
	bInventoryMenuOpen = false;

	if (!OwningController.IsValid()) return;

	FInputModeGameOnly InputMode;
	OwningController->SetInputMode(InputMode);
	OwningController->SetShowMouseCursor(false);
}

