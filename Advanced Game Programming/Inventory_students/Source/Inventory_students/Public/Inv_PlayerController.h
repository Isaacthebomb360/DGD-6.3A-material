// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "Inv_PlayerController.generated.h"

class UInputMappingContext;
class UInputAction;
class UInv_HUDWidget;
class UInv_InventoryComponent;

/**
 * 
 */
UCLASS()
class INVENTORY_STUDENTS_API AInv_PlayerController : public APlayerController
{
	GENERATED_BODY()

public:
	AInv_PlayerController();

	virtual void Tick(float DeltaTime) override;

	UFUNCTION(BlueprintCallable, Category = "Inventory")
	void ToggleInventory();

protected:
	virtual void BeginPlay();
	virtual void SetupInputComponent() override;

private:
	void PrimaryInteract();
	void CreateHUDWidget();
	void TraceForItem();

	TWeakObjectPtr<UInv_InventoryComponent> InventoryComponent;

	UPROPERTY(EditAnywhere, Category = "Inventory")
	TArray<UInputMappingContext*> DefaultIMCs;

	UPROPERTY(EditAnywhere, Category = "Inventory")
	TObjectPtr<UInputAction> PrimaryInteractAction;

	UPROPERTY(EditAnywhere, Category = "Inventory")
	TObjectPtr<UInputAction> ToggleInventoryAction;

	UPROPERTY()
	TObjectPtr<UInv_HUDWidget> HUDWidget;

	UPROPERTY(EditDefaultsOnly, Category = "Inventory")
	TSubclassOf<UInv_HUDWidget> HUDWidgetClass;

	UPROPERTY(EditDefaultsOnly, Category = "Inventory")
	TEnumAsByte<ECollisionChannel> ItemTraceChannel;

	UPROPERTY(EditDefaultsOnly, Category = "Inventory")
	double TraceLength;

	TWeakObjectPtr<AActor> ThisActor;
	TWeakObjectPtr<AActor> LastActor;
};
