// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameplayDebuggerCategory.h"

class FGameplayDebuggerCategory_Generic : public FGameplayDebuggerCategory
{
public: 
    FGameplayDebuggerCategory_Generic();

    static TSharedRef<FGameplayDebuggerCategory> MakeInstance();

    void CollectData(APlayerController* OwnerPC, AActor* DebugActor) override;

    void DrawData(APlayerController* OwnerPC,
        FGameplayDebuggerCanvasContext& CanvasContext) override;

private:
    FString ActorName;
    FTransform CameraTransform;

    float TankHealth;
};

