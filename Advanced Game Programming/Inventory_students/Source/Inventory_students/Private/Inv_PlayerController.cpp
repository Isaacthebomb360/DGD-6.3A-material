// Fill out your copyright notice in the Description page of Project Settings.


#include "Inv_PlayerController.h"
#include "EnhancedInputSubsystems.h"
#include "EnhancedInputComponent.h"
#include "Inv_HUDWidget.h"

AInv_PlayerController::AInv_PlayerController()
{
    PrimaryActorTick.bCanEverTick = true;
}

void AInv_PlayerController::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);
}

void AInv_PlayerController::BeginPlay()
{
    Super::BeginPlay();

    //add the mapping contexts (IMC)

    UEnhancedInputLocalPlayerSubsystem* Subsystem =
        ULocalPlayer::GetSubsystem<UEnhancedInputLocalPlayerSubsystem>(GetLocalPlayer());

    if (IsValid(Subsystem))
    {
        for (auto* CurrentContext : DefaultIMCs)
        {
            Subsystem->AddMappingContext(CurrentContext, 0);
        }
    }

    CreateHUDWidget();

}

void AInv_PlayerController::SetupInputComponent()
{
    Super::SetupInputComponent();

    UEnhancedInputComponent* EnhancedInputComponent =
        CastChecked<UEnhancedInputComponent>(InputComponent);

    EnhancedInputComponent->BindAction(
        PrimaryInteractAction, ETriggerEvent::Started, this,
        &AInv_PlayerController::PrimaryInteract);
}

void AInv_PlayerController::PrimaryInteract()
{
    UE_LOG(LogTemp, Log, TEXT("Primary Interact"));
}

void AInv_PlayerController::CreateHUDWidget()
{
    HUDWidget = CreateWidget<UInv_HUDWidget>(this, HUDWidgetClass);

    if (IsValid(HUDWidget))
    {
        HUDWidget->AddToViewport();
    }
}
