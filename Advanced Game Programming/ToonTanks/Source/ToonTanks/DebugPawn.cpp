// Fill out your copyright notice in the Description page of Project Settings.


#include "DebugPawn.h"
#include "GameplayDebugger.h"
#include "GameplayDebuggerCategory_Generic.h"

// Sets default values
ADebugPawn::ADebugPawn()
{
 	// Set this pawn to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ADebugPawn::BeginPlay()
{
	Super::BeginPlay();

	IGameplayDebugger& GameplayDebuggerModule = IGameplayDebugger::Get();

	GameplayDebuggerModule.RegisterCategory(
		"Generic",
		IGameplayDebugger::FOnGetCategory::CreateStatic(
			&FGameplayDebuggerCategory_Generic::MakeInstance),
		EGameplayDebuggerCategoryState::EnabledInGameAndSimulate, 6);

	GameplayDebuggerModule.NotifyCategoriesChanged();
	
}

// Called every frame
void ADebugPawn::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

