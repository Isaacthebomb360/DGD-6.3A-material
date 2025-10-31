// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "BasePawn.h"
#include "VisualLogger/VisualLoggerDebugSnapshotInterface.h"
#include "Tank.generated.h"

class USpringArmComponent;
class UCameraComponent;

/**
 * 
 */
UCLASS()
class TOONTANKS_API ATank : public ABasePawn, public IVisualLoggerDebugSnapshotInterface
{
	GENERATED_BODY()

public:

	ATank();

	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

	virtual void Tick(float DeltaTime) override;

	APlayerController* GetTankPlayerController() const { return TankPlayerController; };

	virtual void HandleDestruction() override;

#if ENABLE_VISUAL_LOG
	virtual void GrabDebugSnapshot(FVisualLogEntry* Snapshot) const override;
#endif

protected:

	virtual void BeginPlay() override;

private:

	UPROPERTY(VisibleAnywhere, Category = "Components")
	USpringArmComponent* SpringArm;

	UPROPERTY(VisibleAnywhere, Category = "Components")
	UCameraComponent* Camera;

	UPROPERTY(EditAnywhere, Category = "Movement")
	float Speed = 200.0f;

	UPROPERTY(EditAnywhere, Category = "Movement")
	float RateTurn = 20.0f; 

	APlayerController* TankPlayerController;

	void Move(float Value);

	void Turn(float Value);

#if ENABLE_VISUAL_LOG
	float VLogTimer = 0.0f;
#endif
};
