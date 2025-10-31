// Fill out your copyright notice in the Description page of Project Settings.


#include "Tank.h"
#include "GameFramework/SpringArmComponent.h"
#include "Camera/CameraComponent.h"
#include "Kismet/GameplayStatics.h"

ATank::ATank()
{
    SpringArm = CreateDefaultSubobject<USpringArmComponent>(TEXT("SpringArm"));

    SpringArm->SetupAttachment(RootComponent);

    Camera = CreateDefaultSubobject<UCameraComponent>(TEXT("Camera"));

    Camera->SetupAttachment(SpringArm);
}

void ATank::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
    Super::SetupPlayerInputComponent(PlayerInputComponent);

    PlayerInputComponent->BindAxis(TEXT("MoveForward"), this, &ATank::Move);

    PlayerInputComponent->BindAxis(TEXT("Turn"), this, &ATank::Turn);

    PlayerInputComponent->BindAction(TEXT("Fire"), IE_Pressed, this, &ATank::Fire);
}

void ATank::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    if (TankPlayerController)
    {
        FHitResult HitResult;
        TankPlayerController->GetHitResultUnderCursor(
            ECollisionChannel::ECC_Visibility,
            false,
            HitResult);

        RotateTurret(HitResult.ImpactPoint);
    }

#if ENABLE_VISUAL_LOG
    VLogTimer += DeltaTime;

    if (VLogTimer >= 1.0f)
    {
        UE_VLOG_ARROW(this, LogTemp, Verbose,
            GetActorLocation(), GetActorLocation() + FVector::UpVector * 100,
            FColor::Green, TEXT("Tank Position"));
        
        VLogTimer = 0.0f;
    }
#endif
}

void ATank::HandleDestruction()
{
    Super::HandleDestruction();

    SetActorHiddenInGame(true);

    SetActorTickEnabled(false);

#if ENABLE_VISUAL_LOG
    UE_VLOG(this, TEXT("TankCategory"), Verbose, TEXT("Tank Destroyed at location (%s)"),
        *GetActorLocation().ToString());

    UE_VLOG_ARROW(this, LogTemp, Verbose, GetActorLocation(),
        GetActorLocation() + FVector::UpVector * 100, FColor::Red, 
        TEXT("Tank Destroyed"));
#endif
}

#if ENABLE_VISUAL_LOG
void ATank::GrabDebugSnapshot(FVisualLogEntry* Snapshot) const
{
    IVisualLoggerDebugSnapshotInterface::GrabDebugSnapshot(Snapshot);

    const FVector Location = GetActorLocation();

    const int32 CategoryIndex = Snapshot->Status.AddZeroed();
    FVisualLogStatusCategory& Category = Snapshot->Status[CategoryIndex];

    Category.Category = TEXT("TankCategory");
    const FName CategoryName = FName(Category.Category);
    Category.Add(TEXT("Location"), FString::Printf(TEXT("%s"), *Location.ToString()));

    Snapshot->AddText(
        FString::Printf(TEXT("Location: %s"), *Location.ToString()),
        CategoryName, 
        ELogVerbosity::Verbose);
}
#endif

void ATank::BeginPlay()
{
    Super::BeginPlay();

    TankPlayerController = Cast<APlayerController>(GetController());
}

void ATank::Move(float Value)
{
    FVector DeltaLocation = FVector::ZeroVector;

    DeltaLocation.X = Value * Speed * UGameplayStatics::GetWorldDeltaSeconds(this);

    AddActorLocalOffset(DeltaLocation, true);
}

void ATank::Turn(float Value)
{
    FRotator DeltaRotation = FRotator::ZeroRotator;

    DeltaRotation.Yaw = Value * RateTurn * UGameplayStatics::GetWorldDeltaSeconds(this);

    AddActorLocalRotation(DeltaRotation, true);
}
