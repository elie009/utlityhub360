@echo off
echo Adding new DTOs to project file...

REM Add the new DTO files to the project
echo     <Compile Include="DTOs\CreateBorrowerDto.cs" /> >> temp_project.txt
echo     <Compile Include="DTOs\CreateLoanDto.cs" /> >> temp_project.txt
echo     <Compile Include="DTOs\CreatePaymentDto.cs" /> >> temp_project.txt

echo DTOs added to project file!
echo Please manually add these lines to UtilityHub360.csproj after line 214:
echo.
echo     <Compile Include="DTOs\CreateBorrowerDto.cs" />
echo     <Compile Include="DTOs\CreateLoanDto.cs" />
echo     <Compile Include="DTOs\CreatePaymentDto.cs" />
echo.
pause
