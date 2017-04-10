set -e

sudo apt-key adv --keyserver hkp://pool.sks-keyservers.net --recv-keys 023EDB0B
echo deb https://dl.bintray.com/gauge/gauge-deb nightly main | sudo tee -a /etc/apt/sources.list
sudo apt-get update
sudo apt-get install gauge
