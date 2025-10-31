// Fill out your copyright notice in the Description page of Project Settings.


#include "GameplayDebuggerCategory_Generic.h"
#include "Camera/CameraComponent.h"
#include "HealthComponent.h"

FGameplayDebuggerCategory_Generic::FGameplayDebuggerCategory_Generic()
{
}

TSharedRef<FGameplayDebuggerCategory> FGameplayDebuggerCategory_Generic::MakeInstance()
{
    return MakeShareable(new FGameplayDebuggerCategory_Generic());
}

void FGameplayDebuggerCategory_Generic::CollectData(APlayerController* OwnerPC, 
    AActor* DebugActor)
{
    if (OwnerPC)
    {
        APawn* Pawn = OwnerPC->GetPawn();

        ActorName = Pawn->GetName();

        const UCameraComponent* PlayerCamera = Pawn->GetComponentByClass<UCameraComponent>();

        CameraTransform = PlayerCamera->GetComponentTransform();

        const UHealthComponent* PlayerHealthComponent = Pawn->GetComponentByClass<UHealthComponent>();

        TankHealth = PlayerHealthComponent->GetHealth();
    }
}

void FGameplayDebuggerCategory_Generic::DrawData(APlayerController* OwnerPC,
    FGameplayDebuggerCanvasContext& CanvasContext)
{
    if (!ActorName.IsEmpty())
    {
        CanvasContext.Printf(TEXT("{yellow}Actor name: {black}%s"), *ActorName);
    }

    CanvasContext.Printf(TEXT("{yellow}Camera Location: {black}%s"),
        *CameraTransform.GetLocation().ToString());

    CanvasContext.Printf(TEXT("{yellow}Camera Rotation: {black}%s"),
        *CameraTransform.GetRotation().Rotator().ToString());

    CanvasContext.Printf(TEXT("{yellow}Player Health: {black}%f"), TankHealth);
}
