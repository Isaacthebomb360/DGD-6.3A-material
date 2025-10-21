// Fill out your copyright notice in the Description page of Project Settings.


#include "Projectile.h"
#include "GameFramework/ProjectileMovementComponent.h"
#include "Kismet/GameplayStatics.h"

// Sets default values
AProjectile::AProjectile()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = false;

	ProjectileMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Projectile Mesh"));

	RootComponent = ProjectileMesh;

	ProjectileMovementComponent = CreateDefaultSubobject<UProjectileMovementComponent>(TEXT("Projectile Movement"));

	ProjectileMovementComponent->MaxSpeed = 1300.0f;
	ProjectileMovementComponent->InitialSpeed = 1300.0f;
}

// Called when the game starts or when spawned
void AProjectile::BeginPlay()
{
	Super::BeginPlay();

	ProjectileMesh->OnComponentHit.AddDynamic(this, &AProjectile::OnHit);
	
}

void AProjectile::OnHit(UPrimitiveComponent* HitComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, FVector NormalImpulse, const FHitResult& Hit)
{
	UE_LOG(LogTemp, Warning, TEXT("Hit %s"), *HitComp->GetName());
	UE_LOG(LogTemp, Warning, TEXT("Hit %s"), *OtherComp->GetName());
	UE_LOG(LogTemp, Warning, TEXT("Hit %s"), *OtherActor->GetName());

	auto MyOwner = GetOwner();

	if (MyOwner == nullptr)
	{
		Destroy();
		return;
	}

	//TODO implement the ApplyDamage logic
	auto MyOwnerInstigator = MyOwner->GetInstigatorController();

	auto DamageType = UDamageType::StaticClass();

	if (OtherActor && OtherActor != this && OtherActor != MyOwner)
	{
		UGameplayStatics::ApplyDamage(
			OtherActor,
			Damage,
			MyOwnerInstigator,
			this,
			DamageType
		);
	}

	Destroy();
}

