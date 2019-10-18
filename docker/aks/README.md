# Sitecore on AKS

...

## Provision AKS

```PowerShell
$subscription = ""
$resourceGroup = ""
$clusterName = ""
$acrName = ""

az login
az account set --subscription $subscription

az extension add --name "aks-preview"

az feature register --name "WindowsPreview" --namespace "Microsoft.ContainerService"
# run until is says "Registered"...
az feature list -o table --query "[?contains(name, 'Microsoft.ContainerService/WindowsPreview')].{Name:name,State:properties.state}"

az feature register --name "MultiAgentpoolPreview" --namespace "Microsoft.ContainerService"
# run until is says "Registered"...
az feature list -o table --query "[?contains(name, 'Microsoft.ContainerService/MultiAgentpoolPreview')].{Name:name,State:properties.state}"

az feature register --name "VMSSPreview" --namespace "Microsoft.ContainerService"
# run until is says "Registered"...
az feature list -o table --query "[?contains(name, 'Microsoft.ContainerService/VMSSPreview')].{Name:name,State:properties.state}"

az provider register --namespace "Microsoft.ContainerService"

az aks create --resource-group $resourceGroup --name $clusterName --node-count 1 --kubernetes-version 1.14.3 --generate-ssh-keys --windows-admin-password "P@zsw7rd1337" --windows-admin-username winadm --enable-vmss --network-plugin azure

az aks nodepool add --resource-group $resourceGroup --cluster-name $clusterName --os-type Windows --name wnp1 --node-count 3 --node-vm-size Standard_D4s_v3 --kubernetes-version 1.14.3

az aks nodepool add --resource-group $resourceGroup --cluster-name $clusterName --os-type Linux --name lnp1 --node-count 1 --node-vm-size Standard_D4s_v3 --kubernetes-version 1.14.3

az aks get-credentials --resource-group $resourceGroup --name $clusterName
kubectl create clusterrolebinding kubernetes-dashboard --clusterrole=cluster-admin --serviceaccount=kube-system:kubernetes-dashboard

# assign ACR

# Get the id of the service principal configured for AKS
$CLIENT_ID=$(az aks show --resource-group $resourceGroup --name $clusterName --query "servicePrincipalProfile.clientId" --output tsv)

# Get the ACR registry resource id
$ACR_ID=$(az acr show --name $acrName --resource-group $resourceGroup --query "id" --output tsv)

# Create role assignment
az role assignment create --assignee $CLIENT_ID --role acrpull --scope $ACR_ID

az aks browse --resource-group $resourceGroup--name $clusterName
```

## Install Helm

```text
kubectl apply -f .\docker\aks\helm-rbac.yaml --validate=false
helm init --service-account tiller --node-selectors "beta.kubernetes.io/os=linux"
```

## Deploy HabitatHome XP

```text
kubectl create namespace sitecore-habitathome-platform  

helm install stable/nginx-ingress --set controller.nodeSelector."beta\.kubernetes\.io/os"=linux --set defaultBackend.nodeSelector."beta\.kubernetes\.io/os"=linux --namespace sitecore-habitathome-platform

kubectl apply -f .\docker\aks\sql.yaml -f .\docker\aks\solr.yaml -f .\docker\aks\cm.yaml -f .\docker\aks\cd.yaml -f .\docker\aks\xconnect.yaml -f .\docker\aks\xconnect-automationengine.yaml -f .\docker\aks\xconnect-indexworker.yaml -f .\docker\aks\ingress.yaml --validate=false --namespace sitecore-habitathome-platform
```
