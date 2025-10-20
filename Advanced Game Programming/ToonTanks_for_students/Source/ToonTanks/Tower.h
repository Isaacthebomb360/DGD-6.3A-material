// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "BasePawn.h"
#include "Tower.generated.h"

/**
 * 
 */

class ATank;

UCLASS()
class TOONTANKS_API ATower : public ABasePawn
{
	GENERATED_BODY()

public: 

	virtual void Tick(float DeltaTime) override;

protected:

	virtual void BeginPlay() override;

private:

	ATank* Tank;
	
	UPROPERTY(EditAnywhere, Category = "Combat")
	float FireRange = 300.f;

	bool InFireRange();
	
	FTimerHandle FireRateTimerHandler;
	float FireRate = 2.f;

	void CheckFireCondition();
};
