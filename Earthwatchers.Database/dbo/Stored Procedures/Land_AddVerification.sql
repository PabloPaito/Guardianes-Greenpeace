CREATE PROCEDURE [dbo].[Land_AddVerification]
	@LandId int,
	@EarthwatcherId int,
	@IsAlert bit
AS
BEGIN
IF NOT EXISTS (select Earthwatcher from Verifications where land = @LandId and earthwatcher = @EarthwatcherId and IsAlert = @IsAlert and IsDeleted = 0)
	                                    BEGIN
		                                    Delete From Verifications where land = @LandId and earthwatcher = @EarthwatcherId
		                                    INSERT INTO Verifications (Land, Earthwatcher, IsAlert)
		                                    VALUES (@LandId, @EarthwatcherId, @IsAlert)
		                                    Select count(Land) From Verifications Where land = @LandId and IsDeleted = 0
	                                    END
                                    ELSE
                                        BEGIN
	                                        SELECT 0
                                    END

                                    Declare @Alert int = (select COUNT(IsAlert) as Alert from Verifications where IsDeleted = 0 and IsAlert = 1 and Land = @LandId)
                                    Declare @Ok int = (select COUNT(IsAlert) as Ok from Verifications where IsDeleted = 0 and IsAlert = 0 and Land = @LandId)
                                    Declare @ActualOwner int = (select top 1 Earthwatcher from EarthwatcherLands where Land = @LandId)
                                    Declare @OwnerVerification int = (select top 1 isAlert from Verifications where IsDeleted = 0 and Earthwatcher = @ActualOwner)
		
                                    update Land 
                                    set LandStatus = case when @Alert > @Ok then 4
					                                      when @Ok > @Alert then 3
					                                      when @ActualOwner != 17 and @OwnerVerification = 1 then 4 
					                                      when @ActualOwner != 17 and @OwnerVerification = 0 then 3 end
                                    where Id = @LandId
END
