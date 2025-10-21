// Fill out your copyright notice in the Description page of Project Settings.


#include "Tower.h"
#include "Kismet/GameplayStatics.h"
#include "Tank.h"

void ATower::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    if (InFireRange())
    {
        RotateTurret(Tank->GetActorLocation());
    }
}

void ATower::HandleDestruction()
{
    Super::HandleDestruction();

    Destroy();
}

void ATower::BeginPlay()
{
    Super::BeginPlay();

    Tank = Cast<ATank>(UGameplayStatics::GetPlayerPawn(this, 0));

    GetWorldTimerManager().SetTimer(
        FireRateTimerHandler, 
        this, 
        &ATower::CheckFireCondition,
        FireRate,
        true
        );
}

bool ATower::InFireRange()
{
    if (Tank)
    {
        //find the distance from the Tank
        float Distance = FVector::Dist(GetActorLocation(), Tank->GetActorLocation());

        return Distance <= FireRange;
    }

    return false;
}

void ATower::CheckFireCondition()
{
    if (InFireRange())
    {
        Fire();
    }
}
