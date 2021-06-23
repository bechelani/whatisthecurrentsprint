# Terraform

## Pre-requisites

1. Azure CLI
2. Terraform CLI

## Initial setup

1. Create Storage account

``` powershell
az group create `
 --name rg-tf-witcs `
 --location "West US" `
 --tags Owner="Michel Bechelani" Project="WhatIsTheCurrentSprint" Created' 'Date="2021-06-22"

az storage account create `
 --name satfwitcs `
 --resource-group rg-tf-witcs `
 --kind StorageV2 `
 --sku Standard_LRS `
 --https-only true `
 --allow-blob-public-access false `
 --tags Owner="Michel Bechelani" Project="WhatIsTheCurrentSprint" Created' 'Date="2021-06-22"
```

2. Create Shared Access Signare (SAS) Token for the storage accoun

3. Create and ignore the backend configuration file

Instead of using Azure Storage Account Access Keys, use short-lived Shared Access Signature (SAS) Tokens. Create a local azure.conf file that looks like this:

```
# azure.conf, must be in .gitignore
storage_account_name="azurestorageaccountname"
container_name="storagecontainername"
key="project.tfstate"
sas_token="?sv=2019-12-12…"
```

Triple check that the azure.conf is added to the .gitignore file so that it is not checked into your code repository.

4. Initialize Terraform

```
terraform init -backend-config=azure.conf
```

## Using Terraform

1. Plan

This will create an execution plan based on the current state.

```
terraform plan --out=./tf.plan
```

2. Apply

This will create or update the resources in Azure.

```
terraform apply -input=false -auto-approve "./tf.plan"
```

3. Destroy

This will destroy all the resources in Azure.

```
terraform destroy -input=false
```
